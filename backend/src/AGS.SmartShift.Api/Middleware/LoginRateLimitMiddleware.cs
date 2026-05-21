using System.Collections.Concurrent;

namespace AGS.SmartShift.Api.Middleware;

/// <summary>Per-IP sliding window limit on POST /api/v1/auth/login (credential stuffing mitigation).</summary>
public sealed class LoginRateLimitMiddleware
{
    private static readonly ConcurrentDictionary<string, LoginAttemptWindow> Windows = new();
    private static DateTime _lastDictionaryCleanupUtc = DateTime.UtcNow;
    private static readonly TimeSpan DictionaryCleanupInterval = TimeSpan.FromMinutes(5);
    private readonly RequestDelegate _next;
    private readonly int _maxAttempts;
    private readonly TimeSpan _window;

    public LoginRateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _maxAttempts = int.TryParse(configuration["Auth:LoginRateLimit:MaxAttempts"], out var max)
            ? max
            : 30;
        var windowSeconds = int.TryParse(configuration["Auth:LoginRateLimit:WindowSeconds"], out var sec)
            ? sec
            : 60;
        _window = TimeSpan.FromSeconds(windowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        MaybeCleanupStaleWindows(DateTime.UtcNow);

        if (IsLoginPost(context))
        {
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var window = Windows.GetOrAdd(key, static _ => new LoginAttemptWindow());
            if (!window.TryAcquire(DateTime.UtcNow, _window, _maxAttempts))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Too many login attempts from this address. Try again later.",
                });
                return;
            }
        }

        await _next(context);
    }

    private static void MaybeCleanupStaleWindows(DateTime now)
    {
        if (now - _lastDictionaryCleanupUtc < DictionaryCleanupInterval)
        {
            return;
        }

        _lastDictionaryCleanupUtc = now;
        var inactiveThreshold = TimeSpan.FromMinutes(10);

        foreach (var entry in Windows)
        {
            if (entry.Value.IsInactive(now, inactiveThreshold))
            {
                Windows.TryRemove(entry.Key, out _);
            }
        }
    }

    private static bool IsLoginPost(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase);

    private sealed class LoginAttemptWindow
    {
        private readonly object _sync = new();
        private readonly Queue<DateTime> _attempts = new();
        private DateTime _lastActivityUtc = DateTime.UtcNow;

        public bool TryAcquire(DateTime now, TimeSpan window, int maxAttempts)
        {
            lock (_sync)
            {
                _lastActivityUtc = now;
                while (_attempts.Count > 0 && now - _attempts.Peek() > window)
                {
                    _attempts.Dequeue();
                }

                if (_attempts.Count >= maxAttempts)
                {
                    return false;
                }

                _attempts.Enqueue(now);
                return true;
            }
        }

        public bool IsInactive(DateTime now, TimeSpan inactiveThreshold) =>
            now - _lastActivityUtc > inactiveThreshold;
    }
}
