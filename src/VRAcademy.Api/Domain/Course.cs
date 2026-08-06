namespace VRAcademy.Api.Domain;

public sealed record Course(
    Guid Id,
    string Code,
    string NameSr,
    string NameEn,
    string DescriptionSr,
    string DescriptionEn,
    int ValidityMonths,
    int PassScore,
    IReadOnlyCollection<Guid> ScenarioIds);

