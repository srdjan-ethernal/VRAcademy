using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Persistence;
using VRAcademy.Api.Persistence.Entities;

namespace VRAcademy.Api.Services;

public static class DemoAccountSeeder
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string DefaultCompanyName = "Ethernal";
    private const string DefaultEmail = "srdjan@ethernal.tech";
    private const string DefaultPassword = "12345";

    public static void EnsureDemoAccount(TrainingDbContext dbContext, IConfiguration configuration)
    {
        if (!configuration.GetValue("DemoAccount:Enabled", true))
        {
            return;
        }

        dbContext.Database.Migrate();

        var email = NormalizeEmail(configuration["DemoAccount:Email"] ?? DefaultEmail);
        var companyName = (configuration["DemoAccount:CompanyName"] ?? DefaultCompanyName).Trim();
        var company = dbContext.Companies.SingleOrDefault(existingCompany => existingCompany.Name == companyName);
        if (company is null)
        {
            company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                SubscriptionLevel = SubscriptionLevel.Enterprise,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.Companies.Add(company);
        }
        else
        {
            company.SubscriptionLevel = SubscriptionLevel.Enterprise;
        }

        var password = HashPassword(configuration["DemoAccount:Password"] ?? DefaultPassword);
        var existingUser = dbContext.Users.SingleOrDefault(user => user.Email == email);
        if (existingUser is not null)
        {
            existingUser.CompanyId = company.Id;
            existingUser.FirstName = configuration["DemoAccount:FirstName"]?.Trim() ?? existingUser.FirstName;
            existingUser.LastName = configuration["DemoAccount:LastName"]?.Trim() ?? existingUser.LastName;
            existingUser.Role = UserRole.SystemAdministrator;
            existingUser.PasswordHash = password.Hash;
            existingUser.PasswordSalt = password.Salt;
            dbContext.AuthSessions.RemoveRange(dbContext.AuthSessions.Where(session => session.UserId == existingUser.Id));
            dbContext.SaveChanges();
            return;
        }

        dbContext.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Email = email,
            FirstName = configuration["DemoAccount:FirstName"]?.Trim() ?? "Srdjan",
            LastName = configuration["DemoAccount:LastName"]?.Trim() ?? "Vukmirovic",
            Role = UserRole.SystemAdministrator,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        });

        dbContext.SaveChanges();
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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
