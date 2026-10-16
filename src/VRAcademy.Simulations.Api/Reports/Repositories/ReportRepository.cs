using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using VRAcademy.Simulations.Api.Reports.Models;
using VRAcademy.Simulations.Api.Reports.Options;

namespace VRAcademy.Simulations.Api.Reports.Repositories;

public sealed class ReportRepository: IReportRepository
{
    private readonly IDistributedCache _cache;
    private readonly ReportCacheOptions _options;
    private readonly ILogger<ReportRepository> _logger;
    
    public ReportRepository(IDistributedCache cache, IOptions<ReportCacheOptions> options, ILogger<ReportRepository> logger)
    {
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }
    
    public Task CreateAsync(Report report, CancellationToken cancellationToken = default) =>
        SaveAsync(report, cancellationToken);

    public async Task<Report?> GetAsync(Guid simulationId, CancellationToken cancellationToken = default)
    {
        byte[]? payload;
        
        try
        {
            payload = await _cache.GetAsync(CacheKey(simulationId), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Failed to read the cached report for simulation {SimulationId}.", simulationId);
            throw;
        }

        if (payload is null) return null;
        
        try
        {
            return JsonSerializer.Deserialize<Report>(payload);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "The cached report for simulation {SimulationId} could not be deserialized.", simulationId);
            throw;
        }
    }

    public Task UpdateAsync(Report report, CancellationToken cancellationToken = default) =>
        SaveAsync(report, cancellationToken);

    private async Task SaveAsync(Report report, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(report);

            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_options.LifetimeHours)
            };

            await _cache.SetAsync(CacheKey(report.SimulationId), payload, entryOptions, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Failed to cache the report for simulation {SimulationId}.", report.SimulationId);
            throw;
        }
    }

    private static string CacheKey(Guid simulationId) => $"reports:{simulationId}";
}
