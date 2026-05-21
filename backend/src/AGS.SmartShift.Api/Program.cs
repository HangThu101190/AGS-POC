using System.Text.Json;
using System.Text.Json.Serialization;
using AGS.SmartShift.Application;
using AGS.SmartShift.Application.Options;
using AGS.SmartShift.Api.HostedServices;
using AGS.SmartShift.Api.Extensions;
using AGS.SmartShift.Api.Hubs;
using AGS.SmartShift.Api.Middleware;
using AGS.SmartShift.Api.Options;
using ApiCorsOptions = AGS.SmartShift.Api.Options.CorsOptions;
using AGS.SmartShift.Infrastructure;
using AGS.SmartShift.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

var isTesting = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Testing",
    StringComparison.OrdinalIgnoreCase);

try
{
    var builder = WebApplication.CreateBuilder(args);
    isTesting = builder.Environment.IsEnvironment("Testing");
    builder.ValidateProductionSecrets();

    if (!isTesting)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
    }

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices()
        .AddSmartShiftAuth(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    builder.Services.AddSignalR();
    builder.Services.Configure<MonitoringOptions>(
        builder.Configuration.GetSection(MonitoringOptions.SectionName));
    builder.Services.Configure<ApiCorsOptions>(
        builder.Configuration.GetSection(ApiCorsOptions.SectionName));

    var corsOrigins = builder.Configuration
        .GetSection(ApiCorsOptions.SectionName)
        .Get<ApiCorsOptions>()?.AllowedOrigins ?? [];
    if (corsOrigins.Length > 0)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                ApiCorsOptions.SectionName,
                policy => policy
                    .WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
    }
    builder.Services.AddHostedService<MonitoringGpsJitterBackgroundService>();
    builder.Services.AddHostedService<MonitoringSignalRBroadcastBackgroundService>();
    if (!isTesting)
    {
        builder.Services.AddHostedService<RefreshTokenCleanupBackgroundService>();
        builder.Services.AddHostedService<FlightImportBackgroundService>();
    }
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "AGS SmartShift API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT access token",
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                },
                Array.Empty<string>()
            },
        });
    });

    var defaultConnection = builder.Configuration.GetConnectionString("Default");

    var healthChecks = builder.Services.AddHealthChecks();
    if (!string.IsNullOrWhiteSpace(defaultConnection))
    {
        healthChecks.AddNpgSql(defaultConnection, name: "postgresql", tags: ["live", "ready"]);
        healthChecks.AddDbContextCheck<SmartShiftDbContext>("efcore", tags: ["live", "ready"]);
    }

    var app = builder.Build();

    var seedDevData = app.Environment.IsDevelopment()
        || app.Environment.IsEnvironment("Testing");
    await SmartShiftDbInitializer.InitializeAsync(app.Services, seedDevData);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!isTesting)
    {
        app.UseSerilogRequestLogging();
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<LoginRateLimitMiddleware>();
    app.UseMiddleware<AcceptLanguageMiddleware>();
    if (corsOrigins.Length > 0)
    {
        app.UseCors(ApiCorsOptions.SectionName);
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<AuditMiddleware>();
    app.MapControllers();
    app.MapHub<MonitoringHub>("/hubs/monitoring");
    app.MapHub<StaffHub>("/hubs/staff");
    app.MapHub<PlanningHub>("/hubs/planning");
    if (!isTesting)
    {
        app.UseHttpMetrics();
        app.MapMetrics("/metrics");
    }

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = static _ => true,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        },
    });
    app.MapHealthChecks("/ready", new HealthCheckOptions
    {
        Predicate = static check => check.Tags.Contains("ready"),
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        },
    });

    app.Run();
}
catch (HostAbortedException)
{
    // Expected when EF Core design-time tools stop the host (dotnet ef database update, migrations add, …).
}
catch (Exception ex)
{
    if (isTesting)
    {
        Console.Error.WriteLine(ex);
    }
    else
    {
        Log.Fatal(ex, "Application terminated unexpectedly");
    }
}
finally
{
    if (!isTesting)
    {
        Log.CloseAndFlush();
    }
}

public partial class Program;
