using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record EnrollmentCompletionResponse(
    Enrollment Enrollment,
    Certificate? Certificate);

