namespace VRAcademy.Api.Domain;

public sealed record TrainingScenario(
    Guid Id,
    string Code,
    string NameSr,
    string NameEn,
    string DescriptionSr,
    string DescriptionEn,
    string RiskCategory,
    int EstimatedMinutes);

