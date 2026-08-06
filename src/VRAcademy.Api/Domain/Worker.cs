namespace VRAcademy.Api.Domain;

public sealed record Worker(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string EmployeeNumber,
    string Department,
    DateTimeOffset CreatedAt);

