using JetBrains.Annotations;

namespace VRAcademy.Simulations.Api.Reports.Options;

[UsedImplicitly]
public sealed class ReportCacheOptions
{
    public const string SectionName = "ReportCache";
    
    public int LifetimeHours { get; set; }
}
