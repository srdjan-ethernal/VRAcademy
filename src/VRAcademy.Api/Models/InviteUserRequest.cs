namespace VRAcademy.Api.Models;

public sealed record InviteUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? TemporaryPassword,
    string? EmployeeNumber,
    string? Department);
