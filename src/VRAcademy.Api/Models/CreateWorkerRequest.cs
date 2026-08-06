namespace VRAcademy.Api.Models;

public sealed record CreateWorkerRequest(
    string FirstName,
    string LastName,
    string? Email,
    string EmployeeNumber,
    string Department);

