using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using VRAcademy.Simulations.Api.Authentication.Options;
using VRAcademy.Simulations.Api.Validation.Extensions;

namespace VRAcademy.Simulations.Api.Authentication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<ApiKeyOptions>(ApiKeyOptions.SectionName);

        services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyDefaults.AuthenticationScheme,
                configureOptions: null);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder(ApiKeyDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }
}
