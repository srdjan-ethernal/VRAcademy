using FluentValidation;
using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteSimulation;

[UsedImplicitly]
public sealed class CompleteSimulationCommandValidator: AbstractValidator<CompleteSimulationCommand>
{
    public CompleteSimulationCommandValidator()
    {
        RuleFor(c => c.FinishedInSeconds)
            .GreaterThan(0);
    }
}
