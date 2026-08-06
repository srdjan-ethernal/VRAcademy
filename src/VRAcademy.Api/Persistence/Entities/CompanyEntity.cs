namespace VRAcademy.Api.Persistence.Entities;

using VRAcademy.Api.Domain;

public sealed class CompanyEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SubscriptionLevel SubscriptionLevel { get; set; } = SubscriptionLevel.SmallBusiness;

    public DateTimeOffset CreatedAt { get; set; }

    public List<UserEntity> Users { get; set; } = new();

    public List<WorkerEntity> Workers { get; set; } = new();
}

