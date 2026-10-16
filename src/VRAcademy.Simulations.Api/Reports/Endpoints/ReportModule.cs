using Carter;
using JetBrains.Annotations;
using Kodster.CQRS;
using Kodster.Results.AspNetCore;
using VRAcademy.Simulations.Api.Reports.Commands.CompleteSimulation;
using VRAcademy.Simulations.Api.Reports.Commands.CompleteStep;
using VRAcademy.Simulations.Api.Reports.Commands.CreateReport;
using VRAcademy.Simulations.Api.Reports.Endpoints.Requests;
using VRAcademy.Simulations.Api.Reports.Queries.GetReport;

namespace VRAcademy.Simulations.Api.Reports.Endpoints;

[UsedImplicitly]
public sealed class ReportModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var reports = app.MapGroup("simulations");

        reports.MapPost("{simulationId:guid}/report", async (
                Guid simulationId,
                CreateSimulationReportRequest request,
                IRequestHandler<CreateReportCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateReportCommand(simulationId, request.SimulationScenario, request.SimulationKind);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            })
            .RequireAuthorization();

        reports.MapGet("{simulationId:guid}/report", async (
                Guid simulationId,
                IRequestHandler<GetReportQuery, ReportResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new GetReportQuery(simulationId), cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .RequireAuthorization();

        reports.MapPatch("{simulationId:guid}/report/completion", async (
                Guid simulationId,
                CompleteSimulationRequest request,
                IRequestHandler<CompleteSimulationCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CompleteSimulationCommand(simulationId, request.IsSuccess, request.FinishedInSeconds);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            })
            .AllowAnonymous();

        reports.MapPost("{simulationId:guid}/report/steps/{stepId:int}", async (
                Guid simulationId,
                int stepId,
                CompleteStepRequest request,
                IRequestHandler<CompleteStepCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CompleteStepCommand(simulationId, stepId, request.IsSuccess, request.FinishedInSeconds);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            })
            .AllowAnonymous();
    }
}
