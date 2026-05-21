using Microsoft.Extensions.Primitives;

namespace AGS.SmartShift.Api.Middleware;

/// <summary>
/// Parses <c>Accept-Language</c> and exposes <c>vi</c> or <c>en</c> for downstream handlers (stub — Sprint 1).
/// </summary>
public sealed class AcceptLanguageMiddleware(RequestDelegate next)
{
    public const string HttpContextItemKey = "RequestLanguage";
    public const string ResponseHeaderName = "Content-Language";

    private static readonly string[] Supported = ["vi", "en"];

    public async Task InvokeAsync(HttpContext context)
    {
        var language = ResolveLanguage(context.Request.Headers.AcceptLanguage);
        context.Items[HttpContextItemKey] = language;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[ResponseHeaderName] = language == "en" ? "en-US" : "vi-VN";
            return Task.CompletedTask;
        });

        await next(context);
    }

    public static string ResolveLanguage(StringValues acceptLanguageHeader)
    {
        if (StringValues.IsNullOrEmpty(acceptLanguageHeader))
        {
            return "vi";
        }

        foreach (var part in acceptLanguageHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tag = part.Split(';', 2)[0].Trim();
            if (tag.Length == 0)
            {
                continue;
            }

            var primary = tag.Split('-', 2)[0].ToLowerInvariant();
            if (Supported.Contains(primary, StringComparer.Ordinal))
            {
                return primary;
            }
        }

        return "vi";
    }
}

public static class HttpContextLanguageExtensions
{
    public static string GetRequestLanguage(this HttpContext context) =>
        context.Items.TryGetValue(AcceptLanguageMiddleware.HttpContextItemKey, out var value) && value is string lang
            ? lang
            : "vi";
}
