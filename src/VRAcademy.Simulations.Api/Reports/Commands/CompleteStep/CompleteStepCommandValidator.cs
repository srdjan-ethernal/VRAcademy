using FluentValidation;
using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Commands.CompleteStep;

[UsedImplicitly]
public sealed class CompleteStepCommandValidator: AbstractValidator<CompleteStepCommand>
{
    public CompleteStepCommandValidator()
    {
        RuleFor(c => c.StepId)
            .GreaterThan(0);
        
        RuleFor(c => c.FinishedInSeconds)
            .GreaterThan(0);
    }
}
