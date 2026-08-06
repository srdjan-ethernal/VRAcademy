using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Persistence.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public CompanyEntity? Company { get; set; }

    public List<AuthSessionEntity> Sessions { get; set; } = new();
}

