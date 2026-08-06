namespace VRAcademy.Api.Models;

public sealed record SendReminderRequest(
    Guid WorkerId,
    string Subject,
    string Message);
