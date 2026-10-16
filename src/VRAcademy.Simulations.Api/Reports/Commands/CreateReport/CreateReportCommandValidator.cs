using FluentValidation;
using JetBrains.Annotations;
using VRAcademy.Simulations.Api.Reports.Models;

namespace VRAcademy.Simulations.Api.Reports.Commands.CreateReport;

[UsedImplicitly]
public sealed class CreateReportCommandValidator: AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(c => c.SimulationId)
            .NotEmpty();

        RuleFor(c => c.SimulationScenario)
            .Must(value => Enum.IsDefined(typeof(SimulationScenario), value))
            .WithMessage("Invalid simulation scenario.");

        RuleFor(c => c.SimulationKind)
            .Must(value => Enum.IsDefined(typeof(SimulationKind), value))
            .WithMessage("Invalid simulation kind.");
    }
}
