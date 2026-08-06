using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Persistence.Entities;

public sealed class EnrollmentEntity
{
    public Guid Id { get; set; }

    public Guid WorkerId { get; set; }

    public Guid CourseId { get; set; }

    public EnrollmentStatus Status { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int? Score { get; set; }

    public int? DurationMinutes { get; set; }

    public WorkerEntity? Worker { get; set; }

    public CourseEntity? Course { get; set; }
}

