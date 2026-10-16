using JetBrains.Annotations;
using Kodster.CQRS;
using Kodster.Results;
using VRAcademy.Simulations.Api.Reports.Errors;
using VRAcademy.Simulations.Api.Reports.Repositories;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteSimulation;

[UsedImplicitly]
public sealed class CompleteSimulationCommandHandler: IRequestHandler<CompleteSimulationCommand>
{
    private readonly IReportRepository _reportRepository;

    public CompleteSimulationCommandHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result> Handle(CompleteSimulationCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetAsync(request.SimulationId, cancellationToken);
        if (report is null) return ReportErrors.ReportNotFound(request.SimulationId);

        if (report.IsFinished) return ReportErrors.SimulationIsFinished;
        if (report.Steps.Count == 0) return ReportErrors.SimulationCannotBeCompleted;
        
        report.FinishSimulation(request.IsSuccess, request.FinishedInSeconds);
        await _reportRepository.UpdateAsync(report, cancellationToken);
        
        return Result.Success();
    }
}
