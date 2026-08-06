namespace VRAcademy.Api.Persistence.Entities;

public sealed class TrainingScenarioEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NameSr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string DescriptionSr { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public string RiskCategory { get; set; } = string.Empty;

    public int EstimatedMinutes { get; set; }

    public List<CourseScenarioEntity> CourseScenarios { get; set; } = new();
}

