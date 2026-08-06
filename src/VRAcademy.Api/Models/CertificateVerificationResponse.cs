using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record CertificateVerificationResponse(
    string CertificateNumber,
    string CourseTitleSr,
    string CourseTitleEn,
    DateTimeOffset IssuedAt,
    DateTimeOffset ValidUntil,
    CertificateStatus Status);
