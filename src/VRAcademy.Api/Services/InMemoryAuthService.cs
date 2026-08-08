using System.Security.Cryptography;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

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
                    SubscriptionLevel.SmallBusiness,
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

    public Result<AuthResponse> LoginWithExternalProvider(ExternalLoginRequest request)
    {
        var validationError = ValidateExternalLogin(request);
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        lock (_lock)
        {
            var storedUser = _users.SingleOrDefault(user =>
                user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            if (storedUser is not null)
            {
                var existingCompany = _companies.Single(company => company.Id == storedUser.CompanyId);
                return Result<AuthResponse>.Success(CreateAuthResponse(storedUser, existingCompany));
            }

            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return Result<AuthResponse>.Failure("Google nalog nije registrovan. Otvorite tab Registracija i unesite naziv kompanije.");
            }

            var companyName = request.CompanyName.Trim();
            var company = _companies.SingleOrDefault(existingCompany =>
                existingCompany.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase));
            if (company is null)
            {
                company = new Company(
                    Guid.NewGuid(),
                    companyName,
                    SubscriptionLevel.SmallBusiness,
                    DateTimeOffset.UtcNow);
                _companies.Add(company);
            }

            var password = HashPassword(CreateTemporaryPassword());
            var account = new UserAccount(
                Guid.NewGuid(),
                company.Id,
                normalizedEmail,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                UserRole.CompanyAdministrator,
                DateTimeOffset.UtcNow);
            storedUser = new StoredUser(account, password.Hash, password.Salt);
            _users.Add(storedUser);

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

    public Result<AuthResponse> ChangePassword(string accessToken, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result<AuthResponse>.Failure("Trenutna i nova lozinka su obavezne.");
        }

        if (request.NewPassword.Length < 5)
        {
            return Result<AuthResponse>.Failure("Lozinka mora imati najmanje 5 karaktera.");
        }

        lock (_lock)
        {
            RemoveExpiredSessions();

            var session = _sessions.SingleOrDefault(existingSession => existingSession.AccessToken == accessToken);
            if (session is null)
            {
                return Result<AuthResponse>.Failure("Sesija nije pronadjena.");
            }

            var userIndex = _users.FindIndex(user => user.Id == session.UserId);
            if (userIndex < 0)
            {
                return Result<AuthResponse>.Failure("Sesija nije pronadjena.");
            }

            var storedUser = _users[userIndex];
            if (!VerifyPassword(request.CurrentPassword, storedUser.PasswordHash, storedUser.PasswordSalt))
            {
                return Result<AuthResponse>.Failure("Trenutna lozinka nije ispravna.");
            }

            var password = HashPassword(request.NewPassword);
            var updatedUser = storedUser with
            {
                PasswordHash = password.Hash,
                PasswordSalt = password.Salt
            };
            _users[userIndex] = updatedUser;
            _sessions.RemoveAll(existingSession => existingSession.UserId == updatedUser.Id);

            var company = _companies.Single(existingCompany => existingCompany.Id == updatedUser.CompanyId);
            return Result<AuthResponse>.Success(CreateAuthResponse(updatedUser, company));
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

    public Result<UserProfileResponse> UpdateUserRole(Guid? companyId, Guid userId, UpdateUserRoleRequest request)
    {
        if (request.Role is UserRole.SystemAdministrator)
        {
            return Result<UserProfileResponse>.Failure("Status zaposlenog moze biti samo Organisation admin ili User.");
        }

        lock (_lock)
        {
            var userIndex = _users.FindIndex(user =>
                user.Id == userId &&
                (!companyId.HasValue || user.CompanyId == companyId.Value));
            if (userIndex < 0)
            {
                return Result<UserProfileResponse>.Failure("Korisnik nije pronadjen.");
            }

            var storedUser = _users[userIndex];
            if (storedUser.Role == UserRole.SystemAdministrator)
            {
                return Result<UserProfileResponse>.Failure("System administrator status se ne menja kroz ovaj ekran.");
            }

            var account = new UserAccount(
                storedUser.Id,
                storedUser.CompanyId,
                storedUser.Email,
                storedUser.FirstName,
                storedUser.LastName,
                request.Role,
                storedUser.CreatedAt);
            var updatedUser = new StoredUser(account, storedUser.PasswordHash, storedUser.PasswordSalt);
            _users[userIndex] = updatedUser;
            _sessions.RemoveAll(session => session.UserId == updatedUser.Id);

            var company = _companies.Single(existingCompany => existingCompany.Id == updatedUser.CompanyId);
            return Result<UserProfileResponse>.Success(ToProfile(updatedUser, company));
        }
    }

    public Result<InvitationResponse> InviteCompanyUser(Guid companyId, InviteUserRequest request, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result<InvitationResponse>.Failure("Email, ime i prezime su obavezni za pozivnicu.");
        }

        var temporaryPassword = string.IsNullOrWhiteSpace(request.TemporaryPassword)
            ? CreateTemporaryPassword()
            : request.TemporaryPassword.Trim();
        var userResult = CreateCompanyUser(companyId, new CreateCompanyUserRequest(
            request.Email,
            temporaryPassword,
            request.FirstName,
            request.LastName,
            UserRole.User));
        if (!userResult.IsSuccess || userResult.Value is null)
        {
            return Result<InvitationResponse>.Failure(userResult.Error ?? "Korisnik nije kreiran.");
        }

        var invitationUrl = $"{baseUrl.TrimEnd('/')}/login.html?email={Uri.EscapeDataString(userResult.Value.Email)}";
        return Result<InvitationResponse>.Success(new InvitationResponse(
            userResult.Value,
            temporaryPassword,
            invitationUrl,
            DateTimeOffset.UtcNow.AddDays(7)));
    }

    public Result<UserProfileResponse> ResetPassword(Guid? companyId, ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result<UserProfileResponse>.Failure("Email i nova lozinka su obavezni.");
        }

        if (request.NewPassword.Length < 5)
        {
            return Result<UserProfileResponse>.Failure("Lozinka mora imati najmanje 5 karaktera.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        lock (_lock)
        {
            var userIndex = _users.FindIndex(user =>
                user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
                (!companyId.HasValue || user.CompanyId == companyId.Value));
            if (userIndex < 0)
            {
                return Result<UserProfileResponse>.Failure("Korisnik nije pronadjen.");
            }

            var storedUser = _users[userIndex];
            var password = HashPassword(request.NewPassword);
            var updatedUser = storedUser with
            {
                PasswordHash = password.Hash,
                PasswordSalt = password.Salt
            };
            _users[userIndex] = updatedUser;
            _sessions.RemoveAll(session => session.UserId == updatedUser.Id);

            var company = _companies.Single(existingCompany => existingCompany.Id == updatedUser.CompanyId);
            return Result<UserProfileResponse>.Success(ToProfile(updatedUser, company));
        }
    }

    public IReadOnlyCollection<CompanyResponse> GetCompanies()
    {
        lock (_lock)
        {
            return _companies
                .Select(ToCompanyResponse)
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
                : Result<CompanyResponse>.Success(ToCompanyResponse(company));
        }
    }

    public Result<CompanyResponse> CreateCompany(CreateCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CompanyResponse>.Failure("Naziv kompanije je obavezan.");
        }

        lock (_lock)
        {
            var normalizedName = request.Name.Trim();
            if (_companies.Any(company => company.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<CompanyResponse>.Failure("Kompanija sa ovim nazivom vec postoji.");
            }

            var company = new Company(
                Guid.NewGuid(),
                normalizedName,
                request.SubscriptionLevel,
                DateTimeOffset.UtcNow);
            _companies.Add(company);

            if (!string.IsNullOrWhiteSpace(request.AdministratorEmail))
            {
                if (string.IsNullOrWhiteSpace(request.AdministratorPassword) ||
                    string.IsNullOrWhiteSpace(request.AdministratorFirstName) ||
                    string.IsNullOrWhiteSpace(request.AdministratorLastName))
                {
                    return Result<CompanyResponse>.Failure("Za administratora kompanije su obavezni email, lozinka, ime i prezime.");
                }

                var normalizedEmail = NormalizeEmail(request.AdministratorEmail);
                if (_users.Any(user => user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<CompanyResponse>.Failure("Korisnik sa email adresom administratora vec postoji.");
                }

                var password = HashPassword(request.AdministratorPassword);
                var account = new UserAccount(
                    Guid.NewGuid(),
                    company.Id,
                    normalizedEmail,
                    request.AdministratorFirstName.Trim(),
                    request.AdministratorLastName.Trim(),
                    UserRole.CompanyAdministrator,
                    DateTimeOffset.UtcNow);
                _users.Add(new StoredUser(account, password.Hash, password.Salt));
            }

            return Result<CompanyResponse>.Success(ToCompanyResponse(company));
        }
    }

    public Result<CompanyResponse> UpdateCompanySubscription(Guid companyId, UpdateCompanySubscriptionRequest request)
    {
        lock (_lock)
        {
            var companyIndex = _companies.FindIndex(company => company.Id == companyId);
            if (companyIndex < 0)
            {
                return Result<CompanyResponse>.Failure("Kompanija nije pronadjena.");
            }

            var updatedCompany = _companies[companyIndex] with { SubscriptionLevel = request.SubscriptionLevel };
            _companies[companyIndex] = updatedCompany;
            return Result<CompanyResponse>.Success(ToCompanyResponse(updatedCompany));
        }
    }

    public Result<CompanyResponse> DeleteCompany(Guid companyId)
    {
        lock (_lock)
        {
            var company = _companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
            if (company is null)
            {
                return Result<CompanyResponse>.Failure("Kompanija nije pronadjena.");
            }

            var response = ToCompanyResponse(company);
            var userIds = _users.Where(user => user.CompanyId == companyId).Select(user => user.Id).ToHashSet();

            _sessions.RemoveAll(session => userIds.Contains(session.UserId));
            _users.RemoveAll(user => user.CompanyId == companyId);
            _companies.RemoveAll(existingCompany => existingCompany.Id == companyId);

            return Result<CompanyResponse>.Success(response);
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

    private static CompanyResponse ToCompanyResponse(Company company)
    {
        return new CompanyResponse(
            company.Id,
            company.Name,
            company.SubscriptionLevel,
            company.CreatedAt);
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

    private static string? ValidateExternalLogin(ExternalLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return "Google nalog nije vratio email adresu.";
        }

        if (!request.Email.Contains('@') || request.Email.Length > 254)
        {
            return "Google email adresa nije ispravna.";
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return "Google nalog nije vratio ime i prezime.";
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

    private static string CreateTemporaryPassword()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(9))
            .Replace("+", "A", StringComparison.Ordinal)
            .Replace("/", "b", StringComparison.Ordinal)
            .TrimEnd('=');
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
