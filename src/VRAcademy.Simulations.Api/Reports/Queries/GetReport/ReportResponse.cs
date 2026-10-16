using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Queries.GetReport;

[UsedImplicitly]
public record ReportResponse(
    Guid SimulationId, 
    int SimulationScenario, 
    int SimulationKind, 
    bool? IsSuccess, 
    float? FinishedInSeconds,
    StepResponse[] Steps);

[UsedImplicitly]
public record StepResponse(int Id, bool IsSuccess, float FinishedInSeconds);
