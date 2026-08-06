namespace VRAcademy.Api.Persistence.Entities;

public sealed class CourseEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NameSr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string DescriptionSr { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public int ValidityMonths { get; set; }

    public int PassScore { get; set; }

    public List<CourseScenarioEntity> CourseScenarios { get; set; } = new();
}

