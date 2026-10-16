using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Authentication.Options;

[UsedImplicitly]
public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";
    public const int MinimumKeyLength = 32;

    public string HeaderName { get; set; } = "X-Api-Key";
    public string Key { get; set; } = string.Empty;
}
