using JetBrains.Annotations;
using Kodster.CQRS;
using Kodster.Results;
using VRAcademy.Simulations.Api.Reports.Errors;
using VRAcademy.Simulations.Api.Reports.Models;
using VRAcademy.Simulations.Api.Reports.Repositories;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteStep;

[UsedImplicitly]
public sealed class CompleteStepCommandHandler: IRequestHandler<CompleteStepCommand>
{
    private readonly IReportRepository _reportRepository;

    public CompleteStepCommandHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetAsync(request.SimulationId, cancellationToken);
        if (report is null) return ReportErrors.ReportNotFound(request.SimulationId);

        if (report.IsFinished) return ReportErrors.SimulationIsFinished;
        if (report.Steps.Any(s => s.Id == request.StepId)) return ReportErrors.StepAlreadyCompleted(request.StepId);
        if (report.Steps.Count >= Report.MaxSteps) return ReportErrors.MaxStepsReached;
        
        report.FinishStep(new Step(request.StepId, request.IsSuccess, request.FinishedInSeconds));
        await _reportRepository.UpdateAsync(report, cancellationToken);
        
        return Result.Success();
    }
}
