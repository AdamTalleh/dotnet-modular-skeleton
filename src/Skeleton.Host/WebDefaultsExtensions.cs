using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Skeleton.Host;

internal sealed class RateLimitingSettings
{
    public const string Section = "RateLimiting";

    [Range(1, int.MaxValue)]
    public int PermitLimit { get; init; } = 100;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;
}

internal static class WebDefaultsExtensions
{
    public static IServiceCollection AddWebDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        // CORS: no origins configured = no policy = cross-origin requests are refused.
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            if (allowedOrigins.Length > 0)
            {
                options.AddDefaultPolicy(policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }
        });

        services.AddOptions<RateLimitingSettings>()
            .BindConfiguration(RateLimitingSettings.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // NOTE: per-instance, per-client-IP fixed window. Behind a proxy set ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
        // so the client IP is real; for limits shared across replicas use a distributed limiter (e.g. Redis) instead.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var settings = context.RequestServices.GetRequiredService<IOptions<RateLimitingSettings>>().Value;
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.PermitLimit,
                    Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                });
            });
        });

        return services;
    }
}
