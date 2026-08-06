namespace VRAcademy.Api.Domain;

public sealed record UserAccount(
    Guid Id,
    Guid CompanyId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    DateTimeOffset CreatedAt);

