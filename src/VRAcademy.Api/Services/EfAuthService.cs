using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;
using VRAcademy.Api.Persistence;
using VRAcademy.Api.Persistence.Entities;

namespace VRAcademy.Api.Services;

public sealed class EfAuthService : IAuthService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    private readonly TrainingDbContext _dbContext;

    public EfAuthService(TrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Result<AuthResponse> Register(RegisterUserRequest request)
    {
        var validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var normalizedCompanyName = request.CompanyName.Trim();

        if (_dbContext.Users.Any(user => user.Email == normalizedEmail))
        {
            return Result<AuthResponse>.Failure("Korisnik sa ovom email adresom vec postoji.");
        }

        var company = _dbContext.Companies.SingleOrDefault(existingCompany =>
            existingCompany.Name == normalizedCompanyName);

        if (company is null)
        {
            company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                Name = normalizedCompanyName,
                SubscriptionLevel = SubscriptionLevel.SmallBusiness,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Companies.Add(company);
        }

        var password = HashPassword(request.Password);
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.CompanyAdministrator,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        };

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        return Result<AuthResponse>.Success(CreateAuthResponse(user, company));
    }

    public Result<AuthResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email i lozinka su obavezni.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = _dbContext.Users
            .Include(existingUser => existingUser.Company)
            .SingleOrDefault(existingUser => existingUser.Email == normalizedEmail);

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Result<AuthResponse>.Failure("Email ili lozinka nisu ispravni.");
        }

        return Result<AuthResponse>.Success(CreateAuthResponse(user, user.Company!));
    }

    public Result<AuthResponse> LoginWithExternalProvider(ExternalLoginRequest request)
    {
        var validationError = ValidateExternalLogin(request);
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = _dbContext.Users
            .Include(existingUser => existingUser.Company)
            .SingleOrDefault(existingUser => existingUser.Email == normalizedEmail);

        if (user?.Company is not null)
        {
            return Result<AuthResponse>.Success(CreateAuthResponse(user, user.Company));
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return Result<AuthResponse>.Failure("Google nalog nije registrovan. Otvorite tab Registracija i unesite naziv kompanije.");
        }

        var companyName = request.CompanyName.Trim();
        var company = _dbContext.Companies.SingleOrDefault(existingCompany => existingCompany.Name == companyName);
        if (company is null)
        {
            company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                SubscriptionLevel = SubscriptionLevel.SmallBusiness,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.Companies.Add(company);
        }

        var password = HashPassword(CreateTemporaryPassword());
        user = new UserEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.CompanyAdministrator,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        };

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        return Result<AuthResponse>.Success(CreateAuthResponse(user, company));
    }

    public Result<UserProfileResponse> GetCurrentUser(string accessToken)
    {
        RemoveExpiredSessions();

        var session = _dbContext.AuthSessions
            .Include(existingSession => existingSession.User)
            .ThenInclude(user => user!.Company)
            .SingleOrDefault(existingSession => existingSession.AccessToken == accessToken);

        if (session?.User?.Company is null)
        {
            return Result<UserProfileResponse>.Failure("Sesija nije pronadjena.");
        }

        return Result<UserProfileResponse>.Success(ToProfile(session.User, session.User.Company));
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

        RemoveExpiredSessions();

        var session = _dbContext.AuthSessions
            .Include(existingSession => existingSession.User)
            .ThenInclude(user => user!.Company)
            .SingleOrDefault(existingSession => existingSession.AccessToken == accessToken);

        if (session?.User?.Company is null)
        {
            return Result<AuthResponse>.Failure("Sesija nije pronadjena.");
        }

        var user = session.User;
        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return Result<AuthResponse>.Failure("Trenutna lozinka nije ispravna.");
        }

        var password = HashPassword(request.NewPassword);
        user.PasswordHash = password.Hash;
        user.PasswordSalt = password.Salt;
        _dbContext.AuthSessions.RemoveRange(_dbContext.AuthSessions.Where(existingSession => existingSession.UserId == user.Id));
        _dbContext.SaveChanges();

        return Result<AuthResponse>.Success(CreateAuthResponse(user, user.Company));
    }

    public IReadOnlyCollection<UserProfileResponse> GetUsersForCompany(Guid companyId)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Include(user => user.Company)
            .Where(user => user.CompanyId == companyId)
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => ToProfile(user, user.Company!))
            .ToList();
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

        var company = _dbContext.Companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
        if (company is null)
        {
            return Result<UserProfileResponse>.Failure("Kompanija nije pronadjena.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        if (_dbContext.Users.Any(user => user.Email == normalizedEmail))
        {
            return Result<UserProfileResponse>.Failure("Korisnik sa ovom email adresom vec postoji.");
        }

        var password = HashPassword(request.Password);
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = request.Role,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        };

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        return Result<UserProfileResponse>.Success(ToProfile(user, company));
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
        var createUserRequest = new CreateCompanyUserRequest(
            request.Email,
            temporaryPassword,
            request.FirstName,
            request.LastName,
            UserRole.User);
        var userResult = CreateCompanyUser(companyId, createUserRequest);
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
        var query = _dbContext.Users.Include(user => user.Company).Where(user => user.Email == normalizedEmail);
        if (companyId.HasValue)
        {
            query = query.Where(user => user.CompanyId == companyId.Value);
        }

        var user = query.SingleOrDefault();
        if (user?.Company is null)
        {
            return Result<UserProfileResponse>.Failure("Korisnik nije pronadjen.");
        }

        var password = HashPassword(request.NewPassword);
        user.PasswordHash = password.Hash;
        user.PasswordSalt = password.Salt;
        _dbContext.AuthSessions.RemoveRange(_dbContext.AuthSessions.Where(session => session.UserId == user.Id));
        _dbContext.SaveChanges();

        return Result<UserProfileResponse>.Success(ToProfile(user, user.Company));
    }

    public IReadOnlyCollection<CompanyResponse> GetCompanies()
    {
        return _dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => ToCompanyResponse(company))
            .ToList();
    }

    public Result<CompanyResponse> GetCompany(Guid companyId)
    {
        var company = _dbContext.Companies
            .AsNoTracking()
            .SingleOrDefault(existingCompany => existingCompany.Id == companyId);

        return company is null
            ? Result<CompanyResponse>.Failure("Kompanija nije pronadjena.")
            : Result<CompanyResponse>.Success(ToCompanyResponse(company));
    }

    public Result<CompanyResponse> CreateCompany(CreateCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CompanyResponse>.Failure("Naziv kompanije je obavezan.");
        }

        var normalizedName = request.Name.Trim();
        if (_dbContext.Companies.Any(company => company.Name == normalizedName))
        {
            return Result<CompanyResponse>.Failure("Kompanija sa ovim nazivom vec postoji.");
        }

        var company = new CompanyEntity
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            SubscriptionLevel = request.SubscriptionLevel,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Companies.Add(company);

        if (!string.IsNullOrWhiteSpace(request.AdministratorEmail))
        {
            if (string.IsNullOrWhiteSpace(request.AdministratorPassword) ||
                string.IsNullOrWhiteSpace(request.AdministratorFirstName) ||
                string.IsNullOrWhiteSpace(request.AdministratorLastName))
            {
                return Result<CompanyResponse>.Failure("Za administratora kompanije su obavezni email, lozinka, ime i prezime.");
            }

            var normalizedEmail = NormalizeEmail(request.AdministratorEmail);
            if (_dbContext.Users.Any(user => user.Email == normalizedEmail))
            {
                return Result<CompanyResponse>.Failure("Korisnik sa email adresom administratora vec postoji.");
            }

            var password = HashPassword(request.AdministratorPassword);
            _dbContext.Users.Add(new UserEntity
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                Email = normalizedEmail,
                FirstName = request.AdministratorFirstName.Trim(),
                LastName = request.AdministratorLastName.Trim(),
                Role = UserRole.CompanyAdministrator,
                CreatedAt = DateTimeOffset.UtcNow,
                PasswordHash = password.Hash,
                PasswordSalt = password.Salt
            });
        }

        _dbContext.SaveChanges();
        return Result<CompanyResponse>.Success(ToCompanyResponse(company));
    }

    public Result<CompanyResponse> UpdateCompanySubscription(Guid companyId, UpdateCompanySubscriptionRequest request)
    {
        var company = _dbContext.Companies.SingleOrDefault(existingCompany => existingCompany.Id == companyId);
        if (company is null)
        {
            return Result<CompanyResponse>.Failure("Kompanija nije pronadjena.");
        }

        company.SubscriptionLevel = request.SubscriptionLevel;
        _dbContext.SaveChanges();
        return Result<CompanyResponse>.Success(ToCompanyResponse(company));
    }

    private AuthResponse CreateAuthResponse(UserEntity user, CompanyEntity company)
    {
        RemoveExpiredSessions();

        var session = new AuthSessionEntity
        {
            AccessToken = CreateAccessToken(),
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.Add(SessionLifetime)
        };

        _dbContext.AuthSessions.Add(session);
        _dbContext.SaveChanges();

        return new AuthResponse(
            session.AccessToken,
            session.ExpiresAt,
            ToProfile(user, company));
    }

    private static UserProfileResponse ToProfile(UserEntity user, CompanyEntity company)
    {
        return new UserProfileResponse(
            user.Id,
            user.CompanyId,
            company.Name,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt);
    }

    private static CompanyResponse ToCompanyResponse(CompanyEntity company)
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
        var expiredSessions = _dbContext.AuthSessions
            .Where(session => session.ExpiresAt <= now)
            .ToList();

        if (expiredSessions.Count == 0)
        {
            return;
        }

        _dbContext.AuthSessions.RemoveRange(expiredSessions);
        _dbContext.SaveChanges();
    }
}
