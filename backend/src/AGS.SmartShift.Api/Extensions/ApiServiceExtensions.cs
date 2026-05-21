using AGS.SmartShift.Api.Services;
using AGS.SmartShift.Application.Common.Interfaces;

namespace AGS.SmartShift.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddSingleton<IStaffShiftNotifier, StaffShiftNotifier>();
        services.AddScoped<IPlanningDayNotifier, PlanningDayNotifier>();
        return services;
    }
}
