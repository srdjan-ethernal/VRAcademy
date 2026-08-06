namespace VRAcademy.Api.Persistence.Entities;

public sealed class CourseScenarioEntity
{
    public Guid CourseId { get; set; }

    public Guid ScenarioId { get; set; }

    public CourseEntity? Course { get; set; }

    public TrainingScenarioEntity? Scenario { get; set; }
}

