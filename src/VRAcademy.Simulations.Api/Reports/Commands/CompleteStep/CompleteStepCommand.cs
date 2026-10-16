using Kodster.CQRS;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteStep;

public sealed record CompleteStepCommand(Guid SimulationId, int StepId, bool IsSuccess, float FinishedInSeconds): IRequest;
