using Kodster.CQRS;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteSimulation;

public sealed record CompleteSimulationCommand(Guid SimulationId, bool IsSuccess, float FinishedInSeconds): IRequest;
