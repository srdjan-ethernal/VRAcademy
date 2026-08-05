using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VRSimulator.Api.Domain;
using VRSimulator.Api.Persistence;
using VRSimulator.Api.Persistence.Entities;

namespace VRSimulator.Api.Services;

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
        if (dbContext.Users.AsNoTracking().Any(user => user.Email == email))
        {
            return;
        }

        var companyName = (configuration["DemoAccount:CompanyName"] ?? DefaultCompanyName).Trim();
        var company = dbContext.Companies.SingleOrDefault(existingCompany => existingCompany.Name == companyName);
        if (company is null)
        {
            company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.Companies.Add(company);
        }

        var password = HashPassword(configuration["DemoAccount:Password"] ?? DefaultPassword);
        dbContext.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Email = email,
            FirstName = configuration["DemoAccount:FirstName"]?.Trim() ?? "Srdjan",
            LastName = configuration["DemoAccount:LastName"]?.Trim() ?? "Vukmirovic",
            Role = UserRole.CompanyAdministrator,
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
