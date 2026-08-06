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

    public IReadOnlyCollection<CompanyResponse> GetCompanies()
    {
        return _dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => new CompanyResponse(company.Id, company.Name, company.CreatedAt))
            .ToList();
    }

    public Result<CompanyResponse> GetCompany(Guid companyId)
    {
        var company = _dbContext.Companies
            .AsNoTracking()
            .SingleOrDefault(existingCompany => existingCompany.Id == companyId);

        return company is null
            ? Result<CompanyResponse>.Failure("Kompanija nije pronadjena.")
            : Result<CompanyResponse>.Success(new CompanyResponse(company.Id, company.Name, company.CreatedAt));
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
