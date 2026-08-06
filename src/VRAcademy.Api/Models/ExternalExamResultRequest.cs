namespace VRAcademy.Api.Models;

public sealed record ExternalExamResultRequest(
    string Status,
    int Score,
    int DurationMinutes);
