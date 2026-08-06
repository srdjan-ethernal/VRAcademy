namespace VRAcademy.Api.Domain;

public sealed record Company(
    Guid Id,
    string Name,
    SubscriptionLevel SubscriptionLevel,
    DateTimeOffset CreatedAt);

