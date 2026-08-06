using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record CreateCompanyRequest(
    string Name,
    SubscriptionLevel SubscriptionLevel,
    string? AdministratorEmail,
    string? AdministratorPassword,
    string? AdministratorFirstName,
    string? AdministratorLastName);
