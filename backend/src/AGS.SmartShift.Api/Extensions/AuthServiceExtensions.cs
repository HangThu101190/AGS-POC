using System.Text;
using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace AGS.SmartShift.Api.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddSmartShiftAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);
        var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (!context.Request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                        {
                            return Task.CompletedTask;
                        }

                        var authorization = context.Request.Headers.Authorization.ToString();
                        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorization["Bearer ".Length..].Trim();
                            return Task.CompletedTask;
                        }

                        var environment = context.HttpContext.RequestServices
                            .GetRequiredService<IWebHostEnvironment>();
                        if (environment.IsDevelopment())
                        {
                            var queryToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(queryToken))
                            {
                                context.Token = queryToken;
                            }
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SmartShiftRoles.PolicyHr, p => p.RequireRole(SmartShiftRoles.Hr));
            options.AddPolicy(SmartShiftRoles.PolicySup, p => p.RequireRole(SmartShiftRoles.Sup));
            options.AddPolicy(SmartShiftRoles.PolicyStaff, p => p.RequireRole(SmartShiftRoles.Staff));
            options.AddPolicy(SmartShiftRoles.PolicyTbdh, p => p.RequireRole(SmartShiftRoles.Tbdh));
            options.AddPolicy(SmartShiftRoles.PolicyShiftLeader, p => p.RequireRole(SmartShiftRoles.ShiftLeader));
            options.AddPolicy(
                SmartShiftRoles.PolicyHrOrSup,
                p => p.RequireRole(SmartShiftRoles.Hr, SmartShiftRoles.Sup));
            options.AddPolicy(
                SmartShiftRoles.PolicyHrOrShiftLeader,
                p => p.RequireRole(SmartShiftRoles.Hr, SmartShiftRoles.ShiftLeader));
            options.AddPolicy(
                SmartShiftRoles.PolicyStaffingView,
                p => p.RequireRole(
                    SmartShiftRoles.Hr,
                    SmartShiftRoles.Tbdh,
                    SmartShiftRoles.ShiftLeader));
            options.AddPolicy(
                SmartShiftRoles.PolicyStaffingTbdh,
                p => p.RequireRole(SmartShiftRoles.Tbdh, SmartShiftRoles.Hr));
            options.AddPolicy(
                SmartShiftRoles.PolicyStaffingAssign,
                p => p.RequireRole(SmartShiftRoles.ShiftLeader, SmartShiftRoles.Hr));
        });

        return services;
    }
}
