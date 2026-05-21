using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Audit;
using AGS.SmartShift.Infrastructure.Persistence;
using System.Security.Claims;

namespace AGS.SmartShift.Api.Middleware;

public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        SmartShiftDbContext db,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        await _next(context);

        if (!ShouldAudit(context.Request.Method, context.Request.Path))
        {
            return;
        }

        var audit = AuditEvent.Create(
            clock.UtcNow,
            user.UserAccountId,
            context.User.FindFirstValue(SmartShiftClaimTypes.EmployeeCode),
            context.Request.Method,
            context.Request.Path.Value ?? "/",
            context.Response.StatusCode);

        try
        {
            db.AuditEvents.Add(audit);
            await db.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception ex)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Audit-Record"] = "failed";
                return Task.CompletedTask;
            });
            _logger.LogError(
                ex,
                "Failed to persist audit event {AuditId} for {Method} {Path}",
                audit.Id,
                audit.HttpMethod,
                audit.Path);
        }
    }

    private static bool ShouldAudit(string method, PathString path)
    {
        if (path.StartsWithSegments("/health") || path.StartsWithSegments("/ready"))
        {
            return false;
        }

        return method is "POST" or "PUT" or "PATCH" or "DELETE";
    }
}
