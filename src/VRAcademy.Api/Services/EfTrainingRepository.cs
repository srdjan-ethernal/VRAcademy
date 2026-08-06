using Microsoft.EntityFrameworkCore;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;
using VRAcademy.Api.Persistence;
using VRAcademy.Api.Persistence.Entities;

namespace VRAcademy.Api.Services;

public sealed class EfTrainingRepository : ITrainingRepository
{
    private readonly TrainingDbContext _dbContext;

    public EfTrainingRepository(TrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<TrainingScenario> GetScenarios()
    {
        return _dbContext.Scenarios
            .AsNoTracking()
            .OrderBy(scenario => scenario.NameSr)
            .Select(ToDomain)
            .ToList();
    }

    public IReadOnlyCollection<Course> GetCourses()
    {
        return _dbContext.Courses
            .AsNoTracking()
            .Include(course => course.CourseScenarios)
            .OrderBy(course => course.NameSr)
            .Select(ToDomain)
            .ToList();
    }

    public IReadOnlyCollection<Worker> GetWorkers(Guid companyId)
    {
        return _dbContext.Workers
            .AsNoTracking()
            .Where(worker => worker.CompanyId == companyId)
            .OrderBy(worker => worker.LastName)
            .ThenBy(worker => worker.FirstName)
            .Select(ToDomain)
            .ToList();
    }

    public Worker? GetWorker(Guid companyId, Guid workerId)
    {
        var worker = _dbContext.Workers
            .AsNoTracking()
            .SingleOrDefault(existingWorker =>
                existingWorker.Id == workerId &&
                existingWorker.CompanyId == companyId);

        return worker is null ? null : ToDomain(worker);
    }

    public Worker? GetWorkerByEmail(Guid companyId, string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var worker = _dbContext.Workers
            .AsNoTracking()
            .SingleOrDefault(existingWorker =>
                existingWorker.CompanyId == companyId &&
                existingWorker.Email.ToLower() == normalizedEmail);

        return worker is null ? null : ToDomain(worker);
    }

    public Result<Worker> CreateWorker(Guid companyId, CreateWorkerRequest request)
    {
        var normalizedEmployeeNumber = request.EmployeeNumber.Trim();

        var employeeNumberExists = _dbContext.Workers.Any(worker =>
            worker.CompanyId == companyId &&
            worker.EmployeeNumber == normalizedEmployeeNumber);

        if (employeeNumberExists)
        {
            return Result<Worker>.Failure("Radnik sa ovim brojem zaposlenog vec postoji u kompaniji.");
        }

        var worker = new WorkerEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email?.Trim() ?? string.Empty,
            EmployeeNumber = normalizedEmployeeNumber,
            Department = request.Department.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Workers.Add(worker);
        _dbContext.SaveChanges();

        return Result<Worker>.Success(ToDomain(worker));
    }

    public IReadOnlyCollection<Enrollment> GetEnrollments(Guid companyId)
    {
        return _dbContext.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Worker)
            .Where(enrollment => enrollment.Worker != null && enrollment.Worker.CompanyId == companyId)
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .Select(ToDomain)
            .ToList();
    }

    public Result<Enrollment> CreateEnrollment(Guid companyId, CreateEnrollmentRequest request)
    {
        var workerExists = _dbContext.Workers.Any(worker =>
            worker.Id == request.WorkerId &&
            worker.CompanyId == companyId);

        if (!workerExists)
        {
            return Result<Enrollment>.Failure("Radnik nije pronadjen.");
        }

        var courseExists = _dbContext.Courses.Any(course => course.Id == request.CourseId);
        if (!courseExists)
        {
            return Result<Enrollment>.Failure("Kurs nije pronadjen.");
        }

        var existingOpenEnrollment = _dbContext.Enrollments.Any(enrollment =>
            enrollment.WorkerId == request.WorkerId &&
            enrollment.CourseId == request.CourseId &&
            (enrollment.Status == EnrollmentStatus.Enrolled || enrollment.Status == EnrollmentStatus.InProgress));

        if (existingOpenEnrollment)
        {
            return Result<Enrollment>.Failure("Radnik vec ima aktivan upis na ovaj kurs.");
        }

        var enrollment = new EnrollmentEntity
        {
            Id = Guid.NewGuid(),
            WorkerId = request.WorkerId,
            CourseId = request.CourseId,
            Status = EnrollmentStatus.Enrolled,
            EnrolledAt = DateTimeOffset.UtcNow,
            DueAt = request.DueAt
        };

        _dbContext.Enrollments.Add(enrollment);
        _dbContext.SaveChanges();

        return Result<Enrollment>.Success(ToDomain(enrollment));
    }

    public Result<Enrollment> StartEnrollment(Guid companyId, Guid workerId, Guid enrollmentId)
    {
        var enrollment = _dbContext.Enrollments
            .Include(existingEnrollment => existingEnrollment.Worker)
            .SingleOrDefault(existingEnrollment =>
                existingEnrollment.Id == enrollmentId &&
                existingEnrollment.WorkerId == workerId &&
                existingEnrollment.Worker != null &&
                existingEnrollment.Worker.CompanyId == companyId);

        if (enrollment is null)
        {
            return Result<Enrollment>.Failure("Upis na kurs nije pronadjen.");
        }

        if (enrollment.Status is EnrollmentStatus.Passed or EnrollmentStatus.Failed)
        {
            return Result<Enrollment>.Failure("Zavrsena obuka ne moze ponovo da se pokrene.");
        }

        enrollment.Status = EnrollmentStatus.InProgress;
        _dbContext.SaveChanges();

        return Result<Enrollment>.Success(ToDomain(enrollment));
    }

    public Result<EnrollmentCompletionResponse> CompleteEnrollment(Guid companyId, Guid enrollmentId, CompleteTrainingRequest request)
    {
        var enrollment = _dbContext.Enrollments
            .Include(existingEnrollment => existingEnrollment.Worker)
            .SingleOrDefault(existingEnrollment =>
                existingEnrollment.Id == enrollmentId &&
                existingEnrollment.Worker != null &&
                existingEnrollment.Worker.CompanyId == companyId);

        if (enrollment is null)
        {
            return Result<EnrollmentCompletionResponse>.Failure("Upis na kurs nije pronadjen.");
        }

        if (enrollment.Status is EnrollmentStatus.Passed or EnrollmentStatus.Failed)
        {
            return Result<EnrollmentCompletionResponse>.Failure("Obuka je vec zavrsena.");
        }

        var course = _dbContext.Courses.Single(course => course.Id == enrollment.CourseId);
        var passed = request.Score >= course.PassScore;

        enrollment.Status = passed ? EnrollmentStatus.Passed : EnrollmentStatus.Failed;
        enrollment.CompletedAt = DateTimeOffset.UtcNow;
        enrollment.Score = request.Score;
        enrollment.DurationMinutes = request.DurationMinutes;

        CertificateEntity? certificate = null;
        if (passed)
        {
            certificate = new CertificateEntity
            {
                Id = Guid.NewGuid(),
                CertificateNumber = CreateCertificateNumber(course.Code),
                WorkerId = enrollment.WorkerId,
                CourseId = enrollment.CourseId,
                IssuedAt = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddMonths(course.ValidityMonths),
                Status = CertificateStatus.Active
            };

            _dbContext.Certificates.Add(certificate);
        }

        _dbContext.SaveChanges();

        return Result<EnrollmentCompletionResponse>.Success(new EnrollmentCompletionResponse(
            ToDomain(enrollment),
            certificate is null ? null : ToDomain(certificate)));
    }

    public DashboardSummaryResponse GetDashboardSummary(Guid companyId)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringUntil = now.AddDays(30);
        var enrollmentStatusCounts = _dbContext.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Worker)
            .Where(enrollment => enrollment.Worker != null && enrollment.Worker.CompanyId == companyId)
            .GroupBy(enrollment => enrollment.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToList();
        var certificateStatusCounts = _dbContext.Certificates
            .AsNoTracking()
            .Include(certificate => certificate.Worker)
            .Where(certificate => certificate.Worker != null && certificate.Worker.CompanyId == companyId)
            .GroupBy(certificate => certificate.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToList();
        var scoredEnrollments = _dbContext.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Worker)
            .Where(enrollment =>
                enrollment.Worker != null &&
                enrollment.Worker.CompanyId == companyId &&
                enrollment.Score.HasValue)
            .Select(enrollment => enrollment.Score!.Value)
            .ToList();

        var passedEnrollmentCount = enrollmentStatusCounts
            .SingleOrDefault(status => status.Status == EnrollmentStatus.Passed)?.Count ?? 0;
        var failedEnrollmentCount = enrollmentStatusCounts
            .SingleOrDefault(status => status.Status == EnrollmentStatus.Failed)?.Count ?? 0;
        var completedEnrollmentCount = passedEnrollmentCount + failedEnrollmentCount;
        var averageScore = scoredEnrollments.Count == 0 ? 0 : Math.Round(scoredEnrollments.Average(), 1);
        var passRate = completedEnrollmentCount == 0 ? 0 : Math.Round((double)passedEnrollmentCount / completedEnrollmentCount * 100, 1);

        return new DashboardSummaryResponse(
            _dbContext.Courses.Count(),
            _dbContext.Workers.Count(worker => worker.CompanyId == companyId),
            enrollmentStatusCounts.Sum(status => status.Count),
            enrollmentStatusCounts
                .Where(status => status.Status is EnrollmentStatus.Enrolled or EnrollmentStatus.InProgress)
                .Sum(status => status.Count),
            passedEnrollmentCount,
            failedEnrollmentCount,
            certificateStatusCounts.Sum(status => status.Count),
            _dbContext.Certificates.Count(certificate =>
                certificate.Worker != null &&
                certificate.Worker.CompanyId == companyId &&
                certificate.Status == CertificateStatus.Active &&
                certificate.ValidUntil >= now),
            _dbContext.Certificates.Count(certificate =>
                certificate.Worker != null &&
                certificate.Worker.CompanyId == companyId &&
                certificate.Status == CertificateStatus.Active &&
                certificate.ValidUntil >= now &&
                certificate.ValidUntil <= expiringUntil),
            _dbContext.Certificates.Count(certificate =>
                certificate.Worker != null &&
                certificate.Worker.CompanyId == companyId &&
                (certificate.Status == CertificateStatus.Expired || certificate.ValidUntil < now)),
            averageScore,
            passRate,
            Enum.GetValues<EnrollmentStatus>()
                .Select(status => new EnrollmentStatusCountResponse(
                    status,
                    enrollmentStatusCounts.SingleOrDefault(item => item.Status == status)?.Count ?? 0))
                .ToList(),
            Enum.GetValues<CertificateStatus>()
                .Select(status => new CertificateStatusCountResponse(
                    status,
                    certificateStatusCounts.SingleOrDefault(item => item.Status == status)?.Count ?? 0))
                .ToList());
    }

    public IReadOnlyCollection<Certificate> GetCertificates(Guid companyId)
    {
        return _dbContext.Certificates
            .AsNoTracking()
            .Include(certificate => certificate.Worker)
            .Where(certificate => certificate.Worker != null && certificate.Worker.CompanyId == companyId)
            .OrderByDescending(certificate => certificate.IssuedAt)
            .Select(ToDomain)
            .ToList();
    }

    public IReadOnlyCollection<Certificate> GetCertificatesForWorker(Guid companyId, Guid workerId)
    {
        var workerBelongsToCompany = _dbContext.Workers.Any(worker =>
            worker.Id == workerId &&
            worker.CompanyId == companyId);

        if (!workerBelongsToCompany)
        {
            return Array.Empty<Certificate>();
        }

        return _dbContext.Certificates
            .AsNoTracking()
            .Where(certificate => certificate.WorkerId == workerId)
            .OrderByDescending(certificate => certificate.IssuedAt)
            .Select(ToDomain)
            .ToList();
    }

    public Certificate? GetCertificate(Guid companyId, Guid certificateId)
    {
        var certificate = _dbContext.Certificates
            .AsNoTracking()
            .Include(existingCertificate => existingCertificate.Worker)
            .SingleOrDefault(existingCertificate =>
                existingCertificate.Id == certificateId &&
                existingCertificate.Worker != null &&
                existingCertificate.Worker.CompanyId == companyId);

        return certificate is null ? null : ToDomain(certificate);
    }

    public CertificateVerificationResponse? VerifyCertificate(string certificateNumber)
    {
        var normalizedNumber = certificateNumber.Trim();
        var certificate = _dbContext.Certificates
            .AsNoTracking()
            .Include(existingCertificate => existingCertificate.Course)
            .SingleOrDefault(existingCertificate => existingCertificate.CertificateNumber == normalizedNumber);

        if (certificate?.Course is null)
        {
            return null;
        }

        return new CertificateVerificationResponse(
            certificate.CertificateNumber,
            certificate.Course.NameSr,
            certificate.Course.NameEn,
            certificate.IssuedAt,
            certificate.ValidUntil,
            certificate.Status);
    }

    private static TrainingScenario ToDomain(TrainingScenarioEntity scenario)
    {
        return new TrainingScenario(
            scenario.Id,
            scenario.Code,
            scenario.NameSr,
            scenario.NameEn,
            scenario.DescriptionSr,
            scenario.DescriptionEn,
            scenario.RiskCategory,
            scenario.EstimatedMinutes);
    }

    private static Course ToDomain(CourseEntity course)
    {
        return new Course(
            course.Id,
            course.Code,
            course.NameSr,
            course.NameEn,
            course.DescriptionSr,
            course.DescriptionEn,
            course.ValidityMonths,
            course.PassScore,
            course.CourseScenarios.Select(courseScenario => courseScenario.ScenarioId).ToList());
    }

    private static Worker ToDomain(WorkerEntity worker)
    {
        return new Worker(
            worker.Id,
            worker.CompanyId,
            worker.FirstName,
            worker.LastName,
            worker.Email,
            worker.EmployeeNumber,
            worker.Department,
            worker.CreatedAt);
    }

    private static Enrollment ToDomain(EnrollmentEntity enrollment)
    {
        return new Enrollment(
            enrollment.Id,
            enrollment.WorkerId,
            enrollment.CourseId,
            enrollment.Status,
            enrollment.EnrolledAt,
            enrollment.DueAt,
            enrollment.CompletedAt,
            enrollment.Score,
            enrollment.DurationMinutes);
    }

    private static Certificate ToDomain(CertificateEntity certificate)
    {
        return new Certificate(
            certificate.Id,
            certificate.CertificateNumber,
            certificate.WorkerId,
            certificate.CourseId,
            certificate.IssuedAt,
            certificate.ValidUntil,
            certificate.Status);
    }

    private static string CreateCertificateNumber(string courseCode)
    {
        return $"SS-{courseCode.ToUpperInvariant()}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    }
}
