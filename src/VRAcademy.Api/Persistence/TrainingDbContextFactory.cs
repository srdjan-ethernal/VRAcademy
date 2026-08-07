using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VRAcademy.Api.Persistence;

public sealed class TrainingDbContextFactory : IDesignTimeDbContextFactory<TrainingDbContext>
{
    public TrainingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
            ?? "Server=(local);Database=Design;Trusted_Connection=True;";

        var optionsBuilder = new DbContextOptionsBuilder<TrainingDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        });

        return new TrainingDbContext(optionsBuilder.Options);
    }
}
