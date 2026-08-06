namespace VRAcademy.Api.Domain;

public sealed record Enrollment(
    Guid Id,
    Guid WorkerId,
    Guid CourseId,
    string ExamId,
    EnrollmentStatus Status,
    DateTimeOffset EnrolledAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? CompletedAt,
    int? Score,
    int? DurationMinutes);

