using FluentValidation;
using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Options;

[UsedImplicitly]
public sealed class ReportCacheOptionsValidator: AbstractValidator<ReportCacheOptions>
{
    public ReportCacheOptionsValidator()
    {
        RuleFor(x => x.LifetimeHours)
            .GreaterThan(0);
    }
}
