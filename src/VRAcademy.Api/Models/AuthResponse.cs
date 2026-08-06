namespace VRAcademy.Api.Models;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserProfileResponse User);

