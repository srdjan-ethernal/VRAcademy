using JetBrains.Annotations;
using Kodster.CQRS;
using Kodster.Results;
using VRAcademy.Simulations.Api.Reports.Errors;
using VRAcademy.Simulations.Api.Reports.Repositories;

namespace VRAcademy.Simulations.Api.Reports.Queries.GetReport;

[UsedImplicitly]
public sealed class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportResponse>
{
    private readonly IReportRepository _reportRepository;

    public GetReportQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ReportResponse>> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetAsync(request.SimulationId, cancellationToken);
        if (report is null) return ReportErrors.ReportNotFound(request.SimulationId);

        return new ReportResponse(
            report.SimulationId,
            (int)report.SimulationScenario,
            (int)report.SimulationKind,
            report.IsSuccess,
            report.FinishedInSeconds,
            report.Steps
                .OrderBy(step => step.Id)
                .Select(step => new StepResponse(step.Id, step.IsSuccess, step.FinishedInSeconds))
                .ToArray());
    }
}
