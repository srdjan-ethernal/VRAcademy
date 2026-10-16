using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Endpoints.Requests;

[UsedImplicitly]
public sealed record CompleteStepRequest(bool IsSuccess, float FinishedInSeconds);
