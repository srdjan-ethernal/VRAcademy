using VRSimulator.Api.Domain;
using VRSimulator.Api.Models;

namespace VRSimulator.Api.Services;

public sealed class InMemoryTrainingRepository : ITrainingRepository
{
    private readonly object _lock = new();
    private readonly List<TrainingScenario> _scenarios;
    private readonly List<Course> _courses;
    private readonly List<Worker> _workers = new();
    private readonly List<Enrollment> _enrollments = new();
    private readonly List<Certificate> _certificates = new();

    public InMemoryTrainingRepository()
    {
        _scenarios = SeedData.CreateScenarios();
        _courses = SeedData.CreateCourses(_scenarios);
    }

    public IReadOnlyCollection<TrainingScenario> GetScenarios() => _scenarios;

    public IReadOnlyCollection<Course> GetCourses() => _courses;

    public IReadOnlyCollection<Worker> GetWorkers(Guid companyId)
    {
        lock (_lock)
        {
            return _workers
                .Where(worker => worker.CompanyId == companyId)
                .ToList();
        }
    }

    public Worker? GetWorker(Guid companyId, Guid workerId)
    {
        lock (_lock)
        {
            return _workers.SingleOrDefault(worker =>
                worker.Id == workerId &&
                worker.CompanyId == companyId);
        }
    }

    public Worker? GetWorkerByEmail(Guid companyId, string email)
    {
        lock (_lock)
        {
            return _workers.SingleOrDefault(worker =>
                worker.CompanyId == companyId &&
                string.Equals(worker.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public Result<Worker> CreateWorker(Guid companyId, CreateWorkerRequest request)
    {
        var normalizedEmployeeNumber = request.EmployeeNumber.Trim();

        lock (_lock)
        {
            var employeeNumberExists = _workers.Any(worker =>
                worker.CompanyId == companyId &&
                worker.EmployeeNumber == normalizedEmployeeNumber);

            if (employeeNumberExists)
            {
                return Result<Worker>.Failure("Radnik sa ovim brojem zaposlenog vec postoji u kompaniji.");
            }
        }

        var worker = new Worker(
            Guid.NewGuid(),
            companyId,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email?.Trim() ?? string.Empty,
            normalizedEmployeeNumber,
            request.Department.Trim(),
            DateTimeOffset.UtcNow);

        lock (_lock)
        {
            _workers.Add(worker);
        }

        return Result<Worker>.Success(worker);
    }

    public IReadOnlyCollection<Enrollment> GetEnrollments(Guid companyId)
    {
        lock (_lock)
        {
            return _enrollments
                .Where(enrollment => WorkerBelongsToCompany(enrollment.WorkerId, companyId))
                .ToList();
        }
    }

    public Result<Enrollment> CreateEnrollment(Guid companyId, CreateEnrollmentRequest request)
    {
        lock (_lock)
        {
            if (!WorkerBelongsToCompany(request.WorkerId, companyId))
            {
                return Result<Enrollment>.Failure("Radnik nije pronadjen.");
            }

            if (_courses.All(course => course.Id != request.CourseId))
            {
                return Result<Enrollment>.Failure("Kurs nije pronadjen.");
            }

            var existingOpenEnrollment = _enrollments.Any(enrollment =>
                enrollment.WorkerId == request.WorkerId &&
                enrollment.CourseId == request.CourseId &&
                enrollment.Status is EnrollmentStatus.Enrolled or EnrollmentStatus.InProgress);

            if (existingOpenEnrollment)
            {
                return Result<Enrollment>.Failure("Radnik vec ima aktivan upis na ovaj kurs.");
            }

            var enrollment = new Enrollment(
                Guid.NewGuid(),
                request.WorkerId,
                request.CourseId,
                EnrollmentStatus.Enrolled,
                DateTimeOffset.UtcNow,
                request.DueAt,
                null,
                null,
                null);

            _enrollments.Add(enrollment);
            return Result<Enrollment>.Success(enrollment);
        }
    }

    public Result<Enrollment> StartEnrollment(Guid companyId, Guid workerId, Guid enrollmentId)
    {
        lock (_lock)
        {
            if (!WorkerBelongsToCompany(workerId, companyId))
            {
                return Result<Enrollment>.Failure("Radnik nije pronadjen.");
            }

            var enrollmentIndex = _enrollments.FindIndex(enrollment =>
                enrollment.Id == enrollmentId &&
                enrollment.WorkerId == workerId);

            if (enrollmentIndex < 0)
            {
                return Result<Enrollment>.Failure("Upis na kurs nije pronadjen.");
            }

            var enrollment = _enrollments[enrollmentIndex];
            if (enrollment.Status is EnrollmentStatus.Passed or EnrollmentStatus.Failed)
            {
                return Result<Enrollment>.Failure("Zavrsena obuka ne moze ponovo da se pokrene.");
            }

            var startedEnrollment = enrollment with { Status = EnrollmentStatus.InProgress };
            _enrollments[enrollmentIndex] = startedEnrollment;

            return Result<Enrollment>.Success(startedEnrollment);
        }
    }

    public Result<EnrollmentCompletionResponse> CompleteEnrollment(Guid companyId, Guid enrollmentId, CompleteTrainingRequest request)
    {
        lock (_lock)
        {
            var enrollmentIndex = _enrollments.FindIndex(enrollment =>
                enrollment.Id == enrollmentId &&
                WorkerBelongsToCompany(enrollment.WorkerId, companyId));

            if (enrollmentIndex < 0)
            {
                return Result<EnrollmentCompletionResponse>.Failure("Upis na kurs nije pronadjen.");
            }

            var enrollment = _enrollments[enrollmentIndex];
            if (enrollment.Status is EnrollmentStatus.Passed or EnrollmentStatus.Failed)
            {
                return Result<EnrollmentCompletionResponse>.Failure("Obuka je vec zavrsena.");
            }

            var course = _courses.Single(course => course.Id == enrollment.CourseId);
            var passed = request.Score >= course.PassScore;

            var completedEnrollment = enrollment with
            {
                Status = passed ? EnrollmentStatus.Passed : EnrollmentStatus.Failed,
                CompletedAt = DateTimeOffset.UtcNow,
                Score = request.Score,
                DurationMinutes = request.DurationMinutes
            };

            _enrollments[enrollmentIndex] = completedEnrollment;

            Certificate? certificate = null;
            if (passed)
            {
                certificate = new Certificate(
                    Guid.NewGuid(),
                    CreateCertificateNumber(course.Code),
                    enrollment.WorkerId,
                    enrollment.CourseId,
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddMonths(course.ValidityMonths),
                    CertificateStatus.Active);

                _certificates.Add(certificate);
            }

            return Result<EnrollmentCompletionResponse>.Success(new EnrollmentCompletionResponse(
                completedEnrollment,
                certificate));
        }
    }

    public DashboardSummaryResponse GetDashboardSummary(Guid companyId)
    {
        lock (_lock)
        {
            var workers = _workers
                .Where(worker => worker.CompanyId == companyId)
                .ToList();
            var workerIds = workers.Select(worker => worker.Id).ToHashSet();
            var enrollments = _enrollments
                .Where(enrollment => workerIds.Contains(enrollment.WorkerId))
                .ToList();
            var certificates = _certificates
                .Where(certificate => workerIds.Contains(certificate.WorkerId))
                .ToList();

            return CreateDashboardSummary(_courses.Count, workers.Count, enrollments, certificates);
        }
    }

    public IReadOnlyCollection<Certificate> GetCertificates(Guid companyId)
    {
        lock (_lock)
        {
            return _certificates
                .Where(certificate => WorkerBelongsToCompany(certificate.WorkerId, companyId))
                .ToList();
        }
    }

    public IReadOnlyCollection<Certificate> GetCertificatesForWorker(Guid companyId, Guid workerId)
    {
        lock (_lock)
        {
            if (!WorkerBelongsToCompany(workerId, companyId))
            {
                return Array.Empty<Certificate>();
            }

            return _certificates
                .Where(certificate => certificate.WorkerId == workerId)
                .ToList();
        }
    }

    public Certificate? GetCertificate(Guid companyId, Guid certificateId)
    {
        lock (_lock)
        {
            return _certificates.SingleOrDefault(certificate =>
                certificate.Id == certificateId &&
                WorkerBelongsToCompany(certificate.WorkerId, companyId));
        }
    }

    public CertificateVerificationResponse? VerifyCertificate(string certificateNumber)
    {
        lock (_lock)
        {
            var certificate = _certificates.SingleOrDefault(existingCertificate =>
                existingCertificate.CertificateNumber == certificateNumber.Trim());
            if (certificate is null)
            {
                return null;
            }

            var course = _courses.SingleOrDefault(existingCourse => existingCourse.Id == certificate.CourseId);
            if (course is null)
            {
                return null;
            }

            return new CertificateVerificationResponse(
                certificate.CertificateNumber,
                course.NameSr,
                course.NameEn,
                certificate.IssuedAt,
                certificate.ValidUntil,
                certificate.Status);
        }
    }

    private bool WorkerBelongsToCompany(Guid workerId, Guid companyId)
    {
        return _workers.Any(worker => worker.Id == workerId && worker.CompanyId == companyId);
    }

    private static DashboardSummaryResponse CreateDashboardSummary(
        int courseCount,
        int workerCount,
        IReadOnlyCollection<Enrollment> enrollments,
        IReadOnlyCollection<Certificate> certificates)
    {
        var activeEnrollmentCount = enrollments.Count(enrollment =>
            enrollment.Status is EnrollmentStatus.Enrolled or EnrollmentStatus.InProgress);
        var passedEnrollmentCount = enrollments.Count(enrollment => enrollment.Status == EnrollmentStatus.Passed);
        var failedEnrollmentCount = enrollments.Count(enrollment => enrollment.Status == EnrollmentStatus.Failed);
        var scoredEnrollments = enrollments
            .Where(enrollment => enrollment.Score.HasValue)
            .ToList();
        var averageScore = scoredEnrollments.Count == 0 ? 0 : Math.Round(scoredEnrollments.Average(enrollment => enrollment.Score!.Value), 1);
        var completedEnrollmentCount = passedEnrollmentCount + failedEnrollmentCount;
        var passRate = completedEnrollmentCount == 0 ? 0 : Math.Round((double)passedEnrollmentCount / completedEnrollmentCount * 100, 1);
        var now = DateTimeOffset.UtcNow;
        var expiringUntil = now.AddDays(30);
        var activeCertificateCount = certificates.Count(certificate =>
            certificate.Status == CertificateStatus.Active &&
            certificate.ValidUntil >= now);
        var expiringCertificateCount = certificates.Count(certificate =>
            certificate.Status == CertificateStatus.Active &&
            certificate.ValidUntil >= now &&
            certificate.ValidUntil <= expiringUntil);
        var expiredCertificateCount = certificates.Count(certificate =>
            certificate.Status == CertificateStatus.Expired ||
            certificate.ValidUntil < now);

        return new DashboardSummaryResponse(
            courseCount,
            workerCount,
            enrollments.Count,
            activeEnrollmentCount,
            passedEnrollmentCount,
            failedEnrollmentCount,
            certificates.Count,
            activeCertificateCount,
            expiringCertificateCount,
            expiredCertificateCount,
            averageScore,
            passRate,
            Enum.GetValues<EnrollmentStatus>()
                .Select(status => new EnrollmentStatusCountResponse(status, enrollments.Count(enrollment => enrollment.Status == status)))
                .ToList(),
            Enum.GetValues<CertificateStatus>()
                .Select(status => new CertificateStatusCountResponse(status, certificates.Count(certificate => certificate.Status == status)))
                .ToList());
    }

    private static string CreateCertificateNumber(string courseCode)
    {
        return $"SS-{courseCode.ToUpperInvariant()}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    }
}

