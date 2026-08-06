namespace VRAcademy.Api.Domain;

public sealed record Certificate(
    Guid Id,
    string CertificateNumber,
    Guid WorkerId,
    Guid CourseId,
    DateTimeOffset IssuedAt,
    DateTimeOffset ValidUntil,
    CertificateStatus Status);

