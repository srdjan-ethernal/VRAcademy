namespace VRAcademy.Api.Models;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string CompanyName);

