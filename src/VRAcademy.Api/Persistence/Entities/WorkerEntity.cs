namespace VRAcademy.Api.Persistence.Entities;

public sealed class WorkerEntity
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public CompanyEntity? Company { get; set; }

    public List<EnrollmentEntity> Enrollments { get; set; } = new();

    public List<CertificateEntity> Certificates { get; set; } = new();
}
