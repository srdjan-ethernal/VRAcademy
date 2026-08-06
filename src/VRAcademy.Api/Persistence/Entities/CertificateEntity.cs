using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Persistence.Entities;

public sealed class CertificateEntity
{
    public Guid Id { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;

    public Guid WorkerId { get; set; }

    public Guid CourseId { get; set; }

    public DateTimeOffset IssuedAt { get; set; }

    public DateTimeOffset ValidUntil { get; set; }

    public CertificateStatus Status { get; set; }

    public WorkerEntity? Worker { get; set; }

    public CourseEntity? Course { get; set; }
}

