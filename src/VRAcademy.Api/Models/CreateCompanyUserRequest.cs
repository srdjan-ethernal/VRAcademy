using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record CreateCompanyUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role);

