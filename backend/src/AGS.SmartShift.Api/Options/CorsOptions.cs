namespace AGS.SmartShift.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Origins allowed for browser clients (e.g. https://smartshift.ags.local). Empty = CORS middleware not applied.</summary>
    public string[] AllowedOrigins { get; init; } = [];
}
