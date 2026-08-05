namespace VRSimulator.Api.Models;

public sealed record CreateEnrollmentRequest(
    Guid WorkerId,
    Guid CourseId,
    DateTimeOffset? DueAt);

