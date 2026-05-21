using System.Net;
using System.Text.Json;
using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AGS.SmartShift.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ForbiddenAccessException ex)
        {
            await WriteJsonAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (DomainException ex)
        {
            var status = ex.Code switch
            {
                "already_checked_in" => HttpStatusCode.Conflict,
                "past_day_read_only" or "plan_no_slots" => HttpStatusCode.UnprocessableEntity,
                _ => HttpStatusCode.BadRequest,
            };
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { title = ex.Message, code = ex.Code }));
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";
            var problem = new ValidationProblemDetails(
                ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (DbUpdateException ex)
        {
            await WriteDataConflictAsync(context, ex.InnerException?.Message ?? ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState is "22001" or "23505")
        {
            await WriteDataConflictAsync(context, ex.MessageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var title = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.";
            await WriteJsonAsync(context, HttpStatusCode.InternalServerError, title);
        }
    }

    private async Task WriteDataConflictAsync(HttpContext context, string devDetail)
    {
        _logger.LogWarning("Database constraint failed for {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
        var title = _environment.IsDevelopment()
            ? devDetail
            : "Could not save changes due to invalid or conflicting data.";
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { title, code = "data_conflict" }));
    }

    private static async Task WriteJsonAsync(HttpContext context, HttpStatusCode status, string title)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { title }));
    }
}
