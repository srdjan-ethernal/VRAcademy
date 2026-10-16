using JetBrains.Annotations;
using Kodster.CQRS;

namespace VRAcademy.Simulations.Api.Reports.Commands.CreateReport;

[UsedImplicitly]
public sealed record CreateReportCommand(
    Guid SimulationId, 
    int SimulationScenario, 
    int SimulationKind): IRequest;
