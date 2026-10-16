using Kodster.CQRS;

namespace VRAcademy.Simulations.Api.Reports.Queries.GetReport;

public sealed record GetReportQuery(Guid SimulationId): IRequest<ReportResponse>;
