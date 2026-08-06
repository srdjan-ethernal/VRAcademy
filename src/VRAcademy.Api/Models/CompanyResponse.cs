namespace VRAcademy.Api.Models;

using VRAcademy.Api.Domain;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    SubscriptionLevel SubscriptionLevel,
    DateTimeOffset CreatedAt);

