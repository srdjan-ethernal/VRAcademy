namespace VRAcademy.Api.Models;

public sealed record ExternalLoginRequest(
    string Email,
    string FirstName,
    string LastName,
    string? CompanyName);
