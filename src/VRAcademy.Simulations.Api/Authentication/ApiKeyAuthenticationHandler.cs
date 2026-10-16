using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VRAcademy.Simulations.Api.Authentication.Options;

namespace VRAcademy.Simulations.Api.Authentication;

[UsedImplicitly]
public sealed class ApiKeyAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptionsMonitor<ApiKeyOptions> _apiKeyOptions;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptionsMonitor<ApiKeyOptions> apiKeyOptions): base(options, logger, encoder)
    {
        _apiKeyOptions = apiKeyOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKeyOptions = _apiKeyOptions.CurrentValue;

        if (!Request.Headers.TryGetValue(apiKeyOptions.HeaderName, out var values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
        {
            return Task.FromResult(AuthenticateResult.Fail($"The {apiKeyOptions.HeaderName} header has to carry exactly one API key."));
        }

        if (!KeysMatch(values[0]!, apiKeyOptions.Key))
        {
            return Task.FromResult(AuthenticateResult.Fail("The presented API key is not valid."));
        }

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Scheme.Name)), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
    
    private static bool KeysMatch(string presented, string configured) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented), Encoding.UTF8.GetBytes(configured));
}
