using System.Diagnostics;
using CarService.Infrastructure.Observability;
using Microsoft.AspNetCore.Routing;

namespace CarService.Api.Middleware;

public sealed class RequestObservabilityMiddleware
{
    private const string RequestIdHeader = "X-Request-Id";
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";
    private const string ServiceName = "car-service";

    private static readonly ActivitySource ActivitySource = new("AutoRent.CarService");

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestObservabilityMiddleware> _logger;
    private readonly ObservabilityLogWriter _logWriter;

    public RequestObservabilityMiddleware(
        RequestDelegate next,
        ILogger<RequestObservabilityMiddleware> logger,
        ObservabilityLogWriter logWriter)
    {
        _next = next;
        _logger = logger;
        _logWriter = logWriter;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = ResolveRequestId(context);
        context.TraceIdentifier = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        var routeLabel = NormalizePathTemplate(context.Request.Path.Value);
        var method = context.Request.Method;
        var startedAt = Stopwatch.GetTimestamp();

        ActivityContext parentContext;
        var hasParentContext = ActivityContext.TryParse(
            context.Request.Headers[TraceParentHeader],
            context.Request.Headers[TraceStateHeader],
            out parentContext);

        using var activity = hasParentContext
            ? ActivitySource.StartActivity($"{method} {routeLabel}", ActivityKind.Server, parentContext)
            : ActivitySource.StartActivity($"{method} {routeLabel}", ActivityKind.Server);

        activity?.SetTag("http.method", method);
        activity?.SetTag("http.route", routeLabel);
        activity?.SetTag("http.target", context.Request.Path.Value);
        activity?.SetTag("request.id", requestId);

        if (!string.IsNullOrWhiteSpace(activity?.Id))
        {
            context.Response.Headers[TraceParentHeader] = activity.Id;
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["requestId"] = requestId,
            ["traceId"] = activity?.TraceId.ToString(),
            ["method"] = method,
            ["path"] = context.Request.Path.Value
        });

        await _next(context);

        var endpoint = context.GetEndpoint() as RouteEndpoint;
        routeLabel = endpoint?.RoutePattern.RawText is { Length: > 0 } rawRoute
            ? NormalizePathTemplate(rawRoute)
            : NormalizePathTemplate(context.Request.Path.Value);

        if (routeLabel is "/healthz")
        {
            return;
        }

        var statusCode = context.Response.StatusCode;
        var durationMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2);
        activity?.SetTag("http.status_code", statusCode);
        activity?.SetStatus(statusCode >= 500 ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

        _logger.LogInformation(
            "HTTP {Method} {Route} completed with {StatusCode} in {DurationMs}ms",
            method,
            routeLabel,
            statusCode,
            durationMs);

        await _logWriter.WriteAsync(new
        {
            timestamp = DateTimeOffset.UtcNow,
            service = ServiceName,
            level = "Information",
            @event = "http_request_completed",
            requestId,
            traceId = activity?.TraceId.ToString(),
            traceParent = activity?.Id,
            method,
            route = routeLabel,
            path = context.Request.Path.Value,
            statusCode,
            durationMs
        }, context.RequestAborted);
    }

    private static string ResolveRequestId(HttpContext context)
    {
        var incoming = context.Request.Headers[RequestIdHeader].ToString().Trim();
        if (!string.IsNullOrWhiteSpace(incoming))
        {
            return incoming.Length <= 128 ? incoming : incoming[..128];
        }

        return Guid.NewGuid().ToString("N");
    }

    private static string NormalizePathTemplate(string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            return "/";
        }

        var path = rawPath.Trim();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        var segments = path
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizePathSegment)
            .ToArray();

        return segments.Length == 0 ? "/" : "/" + string.Join('/', segments);
    }

    private static string NormalizePathSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return "{empty}";
        }

        return Guid.TryParse(segment, out _) || long.TryParse(segment, out _)
            ? "{id}"
            : segment.Trim().ToLowerInvariant();
    }
}
