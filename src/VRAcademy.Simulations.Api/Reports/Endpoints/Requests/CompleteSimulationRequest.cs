using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Endpoints.Requests;

[UsedImplicitly]
public sealed record CompleteSimulationRequest(bool IsSuccess, float FinishedInSeconds);
