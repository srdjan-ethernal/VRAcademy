using VRAcademy.Api.Persistence;
using VRAcademy.Api.Persistence.Entities;

namespace VRAcademy.Api.Services;

internal static class UserWorkerProvisioner
{
    public static void EnsureWorker(TrainingDbContext dbContext, UserEntity user)
    {
        var normalizedEmail = user.Email.Trim().ToLowerInvariant();
        var worker = dbContext.Workers.FirstOrDefault(existingWorker =>
            existingWorker.CompanyId == user.CompanyId &&
            existingWorker.Email.ToLower() == normalizedEmail);

        if (worker is not null)
        {
            worker.FirstName = user.FirstName;
            worker.LastName = user.LastName;
            worker.Email = user.Email;
            return;
        }

        dbContext.Workers.Add(new WorkerEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = user.CompanyId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            EmployeeNumber = CreateEmployeeNumber(user.Id),
            Department = "-",
            CreatedAt = user.CreatedAt
        });
    }

    public static string CreateEmployeeNumber(Guid userId)
    {
        return $"USR-{userId:N}"[..16];
    }
}
