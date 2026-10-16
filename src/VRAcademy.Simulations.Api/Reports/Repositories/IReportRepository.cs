using VRAcademy.Simulations.Api.Reports.Models;

namespace VRAcademy.Simulations.Api.Reports.Repositories;

public interface IReportRepository
{
    Task CreateAsync(Report report, CancellationToken cancellationToken = default);
    Task<Report?> GetAsync(Guid simulationId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);
}
