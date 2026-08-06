namespace VRAcademy.Api.Models;

public sealed record ReminderResponse(
    Guid WorkerId,
    string Email,
    DateTimeOffset SentAt);
