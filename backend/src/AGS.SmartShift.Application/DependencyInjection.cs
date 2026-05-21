using System.Reflection;
using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AGS.SmartShift.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IResourceAccessService, ResourceAccessService>();
        services.AddScoped<AuthUserDtoBuilder>();
        return services;
    }
}
