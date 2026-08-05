using System.Security.Cryptography;
using VRSimulator.Api.Domain;
using VRSimulator.Api.Models;

namespace VRSimulator.Api.Services;

public sealed class InMemoryAuthService : IAuthService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    private readonly object _lock = new();
    private readonly List<Company> _companies = new();
    private readonly List<StoredUser> _users = new();
    private readonly List<AuthSession> _sessions = new();

    public Result<AuthResponse> Register(RegisterUserRequest request)
    {
        var validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var normalizedCompanyName = request.CompanyName.Trim();

        lock (_lock)
        {
            if (_users.Any(user => user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<AuthResponse>.Failure("Korisnik sa ovom email adresom vec postoji.");
            }

            var company = _companies.SingleOrDefault(existingCompany =>
                existingCompany.Name.Equals(normalizedCompanyName, StringComparison.OrdinalIgnoreCase));

            if (company is null)
            {
                company = new Company(
                    Guid.NewGuid(),
                    normalizedCompanyName,
                    DateTimeOffset.UtcNow);

                _companies.Add(company);
            }

            var password = HashPassword(request.Password);
            var account = new UserAccount(
                Guid.NewGuid(),
                company.Id,
                normalizedEmail,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                UserRole.CompanyAdministrator,
                DateTimeOffset.UtcNow);

            var storedUser = new StoredUser(account, password.Hash, password.Salt);
            _users.Add(storedUser);

            return Result<AuthResponse>.Success(CreateAuthResponse(storedUser, company));
        }
    }

    public Result<AuthResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email i lozinka su obavezni.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        lock (_lock)
        {
            var storedUser = _users.SingleOrDefault(user =>
                user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (storedUser is null || !VerifyPassword(request.Password, storedUser.PasswordHash, storedUser.PasswordSalt))
            {
                return Result<AuthResponse>.Failure("Email ili lozinka nisu ispravni.");
            }

            var company = _companies.Single(company => company.Id == storedUser.CompanyId);
            return Result<AuthResponse>.Success(CreateAuthResponse(storedUser, company));
        }
    }

    public Result<UserProfileResponse> GetCurrentUser(string accessToken)
    {
        lock (_lock)
        {
            RemoveExpiredSessions();

            var session = _sessions.SingleOrDefault(existingSession =>
                existingSession.AccessToken == accessToken);

            if (session is null)
            {
                return Result<UserProfileResponse>.Failure("Sesija nije pronadjena.");
            }

            var storedUser = _users.SingleOrDefault(user => user.Id == session.UserId);
            if (storedUser is null)
            {
                return Result<UserProfileResponse>.Failure("Korisnik nije pronadjen.");
            }

            var company = _companies.Single(company => company.Id == storedUser.CompanyId);
            return Result<UserProfileResponse>.Success(ToProfile(storedUser, company));
        }
    }

    public IReadOnlyCollection<UserProfileResponse> GetUsersForCompany(Guid companyId)
    {
        lock (_lock)
        {
            var company = _companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
            if (company is null)
            {
                return Array.Empty<UserProfileResponse>();
            }

            return _users
                .Where(user => user.CompanyId == companyId)
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .Select(user => ToProfile(user, company))
                .ToList();
        }
    }

    public Result<UserProfileResponse> CreateCompanyUser(Guid companyId, CreateCompanyUserRequest request)
    {
        var validationError = ValidateCompanyUser(request);
        if (validationError is not null)
        {
            return Result<UserProfileResponse>.Failure(validationError);
        }

        if (request.Role == UserRole.SystemAdministrator || request.Role == UserRole.CompanyAdministrator)
        {
            return Result<UserProfileResponse>.Failure("Novi korisnik kroz ovu rutu moze dobiti samo User ulogu.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        lock (_lock)
        {
            var company = _companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
            if (company is null)
            {
                return Result<UserProfileResponse>.Failure("Kompanija nije pronadjena.");
            }

            if (_users.Any(user => user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<UserProfileResponse>.Failure("Korisnik sa ovom email adresom vec postoji.");
            }

            var password = HashPassword(request.Password);
            var account = new UserAccount(
                Guid.NewGuid(),
                companyId,
                normalizedEmail,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.Role,
                DateTimeOffset.UtcNow);

            var storedUser = new StoredUser(account, password.Hash, password.Salt);
            _users.Add(storedUser);

            return Result<UserProfileResponse>.Success(ToProfile(storedUser, company));
        }
    }

    public IReadOnlyCollection<CompanyResponse> GetCompanies()
    {
        lock (_lock)
        {
            return _companies
                .Select(company => new CompanyResponse(company.Id, company.Name, company.CreatedAt))
                .ToList();
        }
    }

    public Result<CompanyResponse> GetCompany(Guid companyId)
    {
        lock (_lock)
        {
            var company = _companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
            return company is null
                ? Result<CompanyResponse>.Failure("Kompanija nije pronadjena.")
                : Result<CompanyResponse>.Success(new CompanyResponse(company.Id, company.Name, company.CreatedAt));
        }
    }

    private AuthResponse CreateAuthResponse(StoredUser storedUser, Company company)
    {
        RemoveExpiredSessions();

        var session = new AuthSession(
            CreateAccessToken(),
            storedUser.Id,
            DateTimeOffset.UtcNow.Add(SessionLifetime));

        _sessions.Add(session);

        return new AuthResponse(
            session.AccessToken,
            session.ExpiresAt,
            ToProfile(storedUser, company));
    }

    private static UserProfileResponse ToProfile(StoredUser storedUser, Company company)
    {
        return new UserProfileResponse(
            storedUser.Id,
            storedUser.CompanyId,
            company.Name,
            storedUser.Email,
            storedUser.FirstName,
            storedUser.LastName,
            storedUser.Role,
            storedUser.CreatedAt);
    }

    private static string? ValidateRegistration(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return "Email, lozinka, ime, prezime i kompanija su obavezni.";
        }

        if (!request.Email.Contains('@') || request.Email.Length > 254)
        {
            return "Email adresa nije ispravna.";
        }

        if (request.Password.Length < 5)
        {
            return "Lozinka mora imati najmanje 5 karaktera.";
        }

        return null;
    }

    private static string? ValidateCompanyUser(CreateCompanyUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return "Email, lozinka, ime i prezime su obavezni.";
        }

        if (!request.Email.Contains('@') || request.Email.Length > 254)
        {
            return "Email adresa nije ispravna.";
        }

        if (request.Password.Length < 5)
        {
            return "Lozinka mora imati najmanje 5 karaktera.";
        }

        return null;
    }

    private static (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return (hash, salt);
    }

    private static bool VerifyPassword(string password, byte[] expectedHash, byte[] salt)
    {
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string CreateAccessToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    }

    private void RemoveExpiredSessions()
    {
        var now = DateTimeOffset.UtcNow;
        _sessions.RemoveAll(session => session.ExpiresAt <= now);
    }

    private sealed record StoredUser(
        Guid Id,
        Guid CompanyId,
        string Email,
        string FirstName,
        string LastName,
        UserRole Role,
        DateTimeOffset CreatedAt,
        byte[] PasswordHash,
        byte[] PasswordSalt)
    {
        public StoredUser(UserAccount account, byte[] passwordHash, byte[] passwordSalt)
            : this(
                account.Id,
                account.CompanyId,
                account.Email,
                account.FirstName,
                account.LastName,
                account.Role,
                account.CreatedAt,
                passwordHash,
                passwordSalt)
        {
        }
    }

    private sealed record AuthSession(
        string AccessToken,
        Guid UserId,
        DateTimeOffset ExpiresAt);
}
