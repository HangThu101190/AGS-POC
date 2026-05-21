namespace AGS.SmartShift.Api.Extensions;

public static class ProductionConfigurationExtensions
{
    private static readonly string[] ForbiddenSigningKeyMarkers =
    [
        "dev-only",
        "change-in-production",
        "ags_dev",
    ];

    public static void ValidateProductionSecrets(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        var signingKey = builder.Configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Production requires Jwt:SigningKey (min 32 chars) via environment or secret store.");
        }

        if (ContainsForbiddenMarker(signingKey))
        {
            throw new InvalidOperationException(
                "Production Jwt:SigningKey must not use development placeholder values.");
        }

        var connectionString = builder.Configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Production requires ConnectionStrings:Default via environment or secret store.");
        }

        if (connectionString.Contains("ags_dev", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Password=ags_dev", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Production database password must not use development credentials.");
        }
    }

    private static bool ContainsForbiddenMarker(string value)
    {
        foreach (var marker in ForbiddenSigningKeyMarkers)
        {
            if (value.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
