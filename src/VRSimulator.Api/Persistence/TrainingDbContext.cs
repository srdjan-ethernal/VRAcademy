using Microsoft.EntityFrameworkCore;
using VRSimulator.Api.Domain;
using VRSimulator.Api.Persistence.Entities;
using VRSimulator.Api.Services;

namespace VRSimulator.Api.Persistence;

public sealed class TrainingDbContext : DbContext
{
    public TrainingDbContext(DbContextOptions<TrainingDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuthSessionEntity> AuthSessions => Set<AuthSessionEntity>();

    public DbSet<CertificateEntity> Certificates => Set<CertificateEntity>();

    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();

    public DbSet<CourseEntity> Courses => Set<CourseEntity>();

    public DbSet<CourseScenarioEntity> CourseScenarios => Set<CourseScenarioEntity>();

    public DbSet<EnrollmentEntity> Enrollments => Set<EnrollmentEntity>();

    public DbSet<TrainingScenarioEntity> Scenarios => Set<TrainingScenarioEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<WorkerEntity> Workers => Set<WorkerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCompanies(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureAuthSessions(modelBuilder);
        ConfigureScenarios(modelBuilder);
        ConfigureCourses(modelBuilder);
        ConfigureCourseScenarios(modelBuilder);
        ConfigureWorkers(modelBuilder);
        ConfigureEnrollments(modelBuilder);
        ConfigureCertificates(modelBuilder);
        SeedCatalog(modelBuilder);
    }

    private static void ConfigureCompanies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyEntity>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(company => company.Id);
            entity.Property(company => company.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(company => company.Name).IsUnique();
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(user => user.PasswordSalt).HasMaxLength(64).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasOne(user => user.Company)
                .WithMany(company => company.Users)
                .HasForeignKey(user => user.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuthSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthSessionEntity>(entity =>
        {
            entity.ToTable("AuthSessions");
            entity.HasKey(session => session.AccessToken);
            entity.Property(session => session.AccessToken).HasMaxLength(200).IsRequired();
            entity.HasOne(session => session.User)
                .WithMany(user => user.Sessions)
                .HasForeignKey(session => session.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScenarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingScenarioEntity>(entity =>
        {
            entity.ToTable("TrainingScenarios");
            entity.HasKey(scenario => scenario.Id);
            entity.Property(scenario => scenario.Code).HasMaxLength(80).IsRequired();
            entity.Property(scenario => scenario.NameSr).HasMaxLength(180).IsRequired();
            entity.Property(scenario => scenario.NameEn).HasMaxLength(180).IsRequired();
            entity.Property(scenario => scenario.DescriptionSr).HasMaxLength(1000).IsRequired();
            entity.Property(scenario => scenario.DescriptionEn).HasMaxLength(1000).IsRequired();
            entity.Property(scenario => scenario.RiskCategory).HasMaxLength(120).IsRequired();
            entity.HasIndex(scenario => scenario.Code).IsUnique();
        });
    }

    private static void ConfigureCourses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseEntity>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(course => course.Id);
            entity.Property(course => course.Code).HasMaxLength(80).IsRequired();
            entity.Property(course => course.NameSr).HasMaxLength(180).IsRequired();
            entity.Property(course => course.NameEn).HasMaxLength(180).IsRequired();
            entity.Property(course => course.DescriptionSr).HasMaxLength(1000).IsRequired();
            entity.Property(course => course.DescriptionEn).HasMaxLength(1000).IsRequired();
            entity.HasIndex(course => course.Code).IsUnique();
        });
    }

    private static void ConfigureCourseScenarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseScenarioEntity>(entity =>
        {
            entity.ToTable("CourseScenarios");
            entity.HasKey(courseScenario => new { courseScenario.CourseId, courseScenario.ScenarioId });
            entity.HasOne(courseScenario => courseScenario.Course)
                .WithMany(course => course.CourseScenarios)
                .HasForeignKey(courseScenario => courseScenario.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(courseScenario => courseScenario.Scenario)
                .WithMany(scenario => scenario.CourseScenarios)
                .HasForeignKey(courseScenario => courseScenario.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWorkers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkerEntity>(entity =>
        {
            entity.ToTable("Workers");
            entity.HasKey(worker => worker.Id);
            entity.Property(worker => worker.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(worker => worker.LastName).HasMaxLength(100).IsRequired();
            entity.Property(worker => worker.Email).HasMaxLength(200).IsRequired();
            entity.Property(worker => worker.EmployeeNumber).HasMaxLength(80).IsRequired();
            entity.Property(worker => worker.Department).HasMaxLength(160).IsRequired();
            entity.HasIndex(worker => new { worker.CompanyId, worker.EmployeeNumber }).IsUnique();
            entity.HasOne(worker => worker.Company)
                .WithMany(company => company.Workers)
                .HasForeignKey(worker => worker.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEnrollments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnrollmentEntity>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(enrollment => enrollment.Id);
            entity.Property(enrollment => enrollment.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(enrollment => enrollment.DueAt);
            entity.HasOne(enrollment => enrollment.Worker)
                .WithMany(worker => worker.Enrollments)
                .HasForeignKey(enrollment => enrollment.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(enrollment => enrollment.Course)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCertificates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificateEntity>(entity =>
        {
            entity.ToTable("Certificates");
            entity.HasKey(certificate => certificate.Id);
            entity.Property(certificate => certificate.CertificateNumber).HasMaxLength(80).IsRequired();
            entity.Property(certificate => certificate.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(certificate => certificate.CertificateNumber).IsUnique();
            entity.HasOne(certificate => certificate.Worker)
                .WithMany(worker => worker.Certificates)
                .HasForeignKey(certificate => certificate.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(certificate => certificate.Course)
                .WithMany()
                .HasForeignKey(certificate => certificate.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedCatalog(ModelBuilder modelBuilder)
    {
        var scenarios = SeedData.CreateScenarios();
        var courses = SeedData.CreateCourses(scenarios);

        modelBuilder.Entity<TrainingScenarioEntity>().HasData(scenarios.Select(scenario => new TrainingScenarioEntity
        {
            Id = scenario.Id,
            Code = scenario.Code,
            NameSr = scenario.NameSr,
            NameEn = scenario.NameEn,
            DescriptionSr = scenario.DescriptionSr,
            DescriptionEn = scenario.DescriptionEn,
            RiskCategory = scenario.RiskCategory,
            EstimatedMinutes = scenario.EstimatedMinutes
        }));

        modelBuilder.Entity<CourseEntity>().HasData(courses.Select(course => new CourseEntity
        {
            Id = course.Id,
            Code = course.Code,
            NameSr = course.NameSr,
            NameEn = course.NameEn,
            DescriptionSr = course.DescriptionSr,
            DescriptionEn = course.DescriptionEn,
            ValidityMonths = course.ValidityMonths,
            PassScore = course.PassScore
        }));

        modelBuilder.Entity<CourseScenarioEntity>().HasData(courses.SelectMany(course =>
            course.ScenarioIds.Select(scenarioId => new CourseScenarioEntity
            {
                CourseId = course.Id,
                ScenarioId = scenarioId
            })));
    }
}
