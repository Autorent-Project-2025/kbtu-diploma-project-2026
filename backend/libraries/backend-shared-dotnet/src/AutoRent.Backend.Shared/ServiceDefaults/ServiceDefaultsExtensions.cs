using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AutoRent.Backend.Shared.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static WebApplicationBuilder AddAutoRentServiceDefaults(
        this WebApplicationBuilder builder,
        string serviceName,
        string? activitySourceName = null)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name is required.", nameof(serviceName));
        }

        builder.Logging.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.ParentId;
        });
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
        });

        builder.Services.AddHealthChecks();
        builder.Services.AddAutoRentOpenTelemetry(
            builder.Configuration,
            builder.Environment.EnvironmentName,
            serviceName,
            activitySourceName);

        return builder;
    }

    public static IServiceCollection AddAutoRentCors(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName = AutoRentDefaults.CorsPolicyName)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    return;
                }

                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }

    public static IEndpointConventionBuilder MapAutoRentHealthChecks(
        this IEndpointRouteBuilder endpoints,
        string path = AutoRentDefaults.HealthPath)
    {
        return endpoints.MapGet(path, () => Microsoft.AspNetCore.Http.Results.Ok(new AutoRentHealthStatus("ok")));
    }

    private static void AddAutoRentOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName,
        string serviceName,
        string? activitySourceName)
    {
        var otlpEndpoint = configuration[AutoRentDefaults.OtlpEndpointConfigurationKey]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            return;
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environmentName
                }))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options => options.RecordException = true);

                if (!string.IsNullOrWhiteSpace(activitySourceName))
                {
                    tracing.AddSource(activitySourceName);
                }

                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = BuildOtlpTracesEndpoint(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });
    }

    private static Uri BuildOtlpTracesEndpoint(string endpoint)
    {
        var uri = new Uri(endpoint, UriKind.Absolute);
        if (uri.AbsolutePath.EndsWith("/v1/traces", StringComparison.OrdinalIgnoreCase))
        {
            return uri;
        }

        var uriBuilder = new UriBuilder(uri);
        var normalizedPath = uriBuilder.Path.TrimEnd('/');
        uriBuilder.Path = string.IsNullOrEmpty(normalizedPath)
            ? "/v1/traces"
            : $"{normalizedPath}/v1/traces";

        return uriBuilder.Uri;
    }

    private sealed record AutoRentHealthStatus(string Status);
}
