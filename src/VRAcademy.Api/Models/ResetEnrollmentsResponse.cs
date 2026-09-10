using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record ResetEnrollmentsResponse(
    int EnrollmentCount,
    int CertificateCount,
    EnrollmentStatus Status);
