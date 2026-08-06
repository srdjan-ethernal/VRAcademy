namespace VRAcademy.Api.Models;

public sealed record InvitationResponse(
    UserProfileResponse User,
    string TemporaryPassword,
    string InvitationUrl,
    DateTimeOffset ExpiresAt);
