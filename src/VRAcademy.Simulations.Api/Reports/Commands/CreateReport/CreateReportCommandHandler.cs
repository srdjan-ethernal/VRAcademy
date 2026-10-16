using JetBrains.Annotations;
using Kodster.CQRS;
using Kodster.Results;
using VRAcademy.Simulations.Api.Reports.Models;
using VRAcademy.Simulations.Api.Reports.Repositories;

namespace VRAcademy.Simulations.Api.Reports.Commands.CreateReport;

[UsedImplicitly]
public sealed class CreateReportCommandHandler: IRequestHandler<CreateReportCommand>
{
    private readonly IReportRepository _reportRepository;

    public CreateReportCommandHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var report = new Report(
            request.SimulationId,
            (SimulationScenario)request.SimulationScenario,
            (SimulationKind)request.SimulationKind,
            null,
            null,
            []);

        await _reportRepository.CreateAsync(report, cancellationToken);
        
        return Result.Success();
    }
}
