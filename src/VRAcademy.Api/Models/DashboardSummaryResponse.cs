using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record DashboardSummaryResponse(
    int CourseCount,
    int WorkerCount,
    int EnrollmentCount,
    int ActiveEnrollmentCount,
    int PassedEnrollmentCount,
    int FailedEnrollmentCount,
    int CertificateCount,
    int ActiveCertificateCount,
    int ExpiringCertificateCount,
    int ExpiredCertificateCount,
    double AverageScore,
    double PassRate,
    IReadOnlyCollection<EnrollmentStatusCountResponse> EnrollmentsByStatus,
    IReadOnlyCollection<CertificateStatusCountResponse> CertificatesByStatus);

public sealed record EnrollmentStatusCountResponse(EnrollmentStatus Status, int Count);

public sealed record CertificateStatusCountResponse(CertificateStatus Status, int Count);
