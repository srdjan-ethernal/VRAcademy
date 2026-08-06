using System.Net;
using System.Net.Mail;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

public sealed class SmtpEmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailNotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Result<ReminderResponse> SendReminder(Worker worker, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(worker.Email))
        {
            return Result<ReminderResponse>.Failure("Radnik nema email adresu.");
        }

        var section = _configuration.GetSection("Email");
        var enabled = section.GetValue<bool>("Enabled");
        var host = section["Host"];
        var fromAddress = section["FromAddress"];

        if (!enabled || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
        {
            return Result<ReminderResponse>.Failure("Email servis nije konfigurisan.");
        }

        using var messageToSend = new MailMessage(fromAddress, worker.Email)
        {
            Subject = subject.Trim(),
            Body = message.Trim(),
            IsBodyHtml = false
        };

        using var smtpClient = new SmtpClient(host, section.GetValue("Port", 587))
        {
            EnableSsl = section.GetValue("EnableSsl", true)
        };

        var userName = section["UserName"];
        var password = section["Password"];
        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
        {
            smtpClient.Credentials = new NetworkCredential(userName, password);
        }

        smtpClient.Send(messageToSend);

        return Result<ReminderResponse>.Success(new ReminderResponse(
            worker.Id,
            worker.Email,
            DateTimeOffset.UtcNow));
    }
}
