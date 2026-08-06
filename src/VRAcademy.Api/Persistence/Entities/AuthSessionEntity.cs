namespace VRAcademy.Api.Persistence.Entities;

public sealed class AuthSessionEntity
{
    public string AccessToken { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public UserEntity? User { get; set; }
}

