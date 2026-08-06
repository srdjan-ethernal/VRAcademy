namespace VRAcademy.Api.Domain;

public sealed record Company(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt);

