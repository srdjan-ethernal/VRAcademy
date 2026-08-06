using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

public interface IEmailNotificationService
{
    Result<ReminderResponse> SendReminder(Worker worker, string subject, string message);
}
