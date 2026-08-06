using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record UserProfileResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    DateTimeOffset CreatedAt);

