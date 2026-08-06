using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

public interface ITrainingRepository
{
    IReadOnlyCollection<TrainingScenario> GetScenarios();

    IReadOnlyCollection<Course> GetCourses();

    IReadOnlyCollection<Worker> GetWorkers(Guid companyId);

    Worker? GetWorker(Guid companyId, Guid workerId);

    Worker? GetWorkerByEmail(Guid companyId, string email);

    Result<Worker> CreateWorker(Guid companyId, CreateWorkerRequest request);

    IReadOnlyCollection<Enrollment> GetEnrollments(Guid companyId);

    Result<Enrollment> CreateEnrollment(Guid companyId, CreateEnrollmentRequest request);

    Result<Enrollment> StartEnrollment(Guid companyId, Guid workerId, Guid enrollmentId);

    Result<EnrollmentCompletionResponse> CompleteEnrollment(Guid companyId, Guid enrollmentId, CompleteTrainingRequest request);

    DashboardSummaryResponse GetDashboardSummary(Guid companyId);

    IReadOnlyCollection<Certificate> GetCertificates(Guid companyId);

    IReadOnlyCollection<Certificate> GetCertificatesForWorker(Guid companyId, Guid workerId);

    Certificate? GetCertificate(Guid companyId, Guid certificateId);

    CertificateVerificationResponse? VerifyCertificate(string certificateNumber);
}

