using VRAcademy.Api.Domain;

namespace VRAcademy.Api.Models;

public sealed record UpdateCompanySubscriptionRequest(
    SubscriptionLevel SubscriptionLevel);
