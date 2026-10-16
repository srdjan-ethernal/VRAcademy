using FluentValidation;
using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Authentication.Options;

[UsedImplicitly]
public sealed class ApiKeyOptionsValidator: AbstractValidator<ApiKeyOptions>
{
    public ApiKeyOptionsValidator()
    {
        RuleFor(x => x.HeaderName)
            .NotEmpty();

        RuleFor(x => x.Key)
            .NotEmpty()
            .MinimumLength(ApiKeyOptions.MinimumKeyLength);
    }
}
