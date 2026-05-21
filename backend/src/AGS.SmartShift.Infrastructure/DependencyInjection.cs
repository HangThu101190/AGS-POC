using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Repositories;
using AGS.SmartShift.Infrastructure.Auth;
using AGS.SmartShift.Infrastructure.Persistence;
using AGS.SmartShift.Infrastructure.Persistence.Interceptors;
using AGS.SmartShift.Infrastructure.Persistence.Repositories;
using AGS.SmartShift.Infrastructure.Export;
using AGS.SmartShift.Infrastructure.Import;
using AGS.SmartShift.Infrastructure.Planning;
using AGS.SmartShift.Infrastructure.Services;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AGS.SmartShift.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = string.Equals(
            configuration["UseInMemoryDatabase"],
            "true",
            StringComparison.OrdinalIgnoreCase);
        services.AddSingleton<RowVersionSaveChangesInterceptor>();

        if (useInMemory)
        {
            var inMemoryName = configuration["InMemoryDatabaseName"] ?? "ags_smartshift_tests";
            services.AddDbContext<SmartShiftDbContext>((sp, options) =>
                options
                    .UseSnakeCaseNamingConvention()
                    .UseInMemoryDatabase(inMemoryName)
                    .AddInterceptors(sp.GetRequiredService<RowVersionSaveChangesInterceptor>()));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<SmartShiftDbContext>((sp, options) =>
                options
                    .UseSnakeCaseNamingConvention()
                    .UseNpgsql(connectionString, npgsql =>
                        npgsql.MigrationsAssembly(typeof(SmartShiftDbContext).Assembly.FullName))
                    .AddInterceptors(sp.GetRequiredService<RowVersionSaveChangesInterceptor>()));
        }

        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IRoleCatalogRepository, RoleCatalogRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IWorkZoneRepository, WorkZoneRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IWeeklyPlanRepository, WeeklyPlanRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IFlightScheduleRepository, FlightScheduleRepository>();
        services.AddScoped<IFlightScheduleDayRepository, FlightScheduleDayRepository>();
        services.AddScoped<IDailyStaffingRepository, DailyStaffingRepository>();
        services.AddScoped<IStaffingConfigRepository, StaffingConfigRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IStaffingSupportQueries, StaffingSupportQueries>();
        services.AddScoped<IDailyStaffingSlotSyncService, DailyStaffingSlotSyncService>();
        services.AddScoped<IPvhkDailyAssignmentExporter, PvhkDailyAssignmentExporter>();
        services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();
        services.AddScoped<ISyncProposalRepository, SyncProposalRepository>();
        services.AddScoped<IPlanningWeekService, PlanningWeekService>();
        services.AddScoped<IFlightExcelParser, FlightExcelParser>();
        services.AddScoped<IFlightImportJobRepository, FlightImportJobRepository>();
        services.AddSingleton<IFlightImportQueue, FlightImportQueue>();
        services.AddScoped<IReconcileWeekExporter, ReconcileWeekExporterService>();
        services.AddScoped<IReconcileMonthExporter, ReconcileMonthExporterService>();
        services.AddScoped<IShiftRevisionRepository, ShiftRevisionRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPermissionCatalogService, PermissionCatalogService>();

        return services;
    }
}
