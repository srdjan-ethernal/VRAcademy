using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Endpoints.Requests;

[UsedImplicitly]
public sealed record CreateSimulationReportRequest(int SimulationScenario, int SimulationKind);
