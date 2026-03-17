using System.Diagnostics;
using IdentityService.Api.Observability;
using Microsoft.AspNetCore.Routing;

namespace IdentityService.Api.Middleware;

public sealed class RequestObservabilityMiddleware
{
    private const string RequestIdHeader = "X-Request-Id";
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";

    private static readonly ActivitySource ActivitySource = new("AutoRent.IdentityService");

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestObservabilityMiddleware> _logger;
    private readonly ObservabilityMetrics _metrics;
    private readonly ObservabilityLogWriter _logWriter;

    public RequestObservabilityMiddleware(
        RequestDelegate next,
        ILogger<RequestObservabilityMiddleware> logger,
        ObservabilityMetrics metrics,
        ObservabilityLogWriter logWriter)
    {
        _next = next;
        _logger = logger;
        _metrics = metrics;
        _logWriter = logWriter;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = ResolveRequestId(context);
        context.TraceIdentifier = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        var routeLabel = ObservabilityMetrics.NormalizePathTemplate(context.Request.Path.Value);
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

        _metrics.IncrementHttpRequestsInFlight();
        try
        {
            await _next(context);
        }
        finally
        {
            var endpoint = context.GetEndpoint() as RouteEndpoint;
            routeLabel = endpoint?.RoutePattern.RawText is { Length: > 0 } rawRoute
                ? ObservabilityMetrics.NormalizePathTemplate(rawRoute)
                : ObservabilityMetrics.NormalizePathTemplate(context.Request.Path.Value);

            var statusCode = context.Response.StatusCode;
            var durationSeconds = Stopwatch.GetElapsedTime(startedAt).TotalSeconds;
            activity?.SetTag("http.status_code", statusCode);
            activity?.SetStatus(statusCode >= 500 ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            var shouldSkipMetrics = routeLabel is "/metrics" or "/healthz";

            if (!shouldSkipMetrics)
            {
                _metrics.RecordHttpRequest(method, routeLabel, statusCode, durationSeconds);

                _logger.LogInformation(
                    "HTTP {Method} {Route} completed with {StatusCode} in {DurationMs}ms",
                    method,
                    routeLabel,
                    statusCode,
                    Math.Round(durationSeconds * 1000, 2));

                await _logWriter.WriteAsync(new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    service = "identity-service",
                    @event = "http_request_completed",
                    requestId,
                    traceId = activity?.TraceId.ToString(),
                    traceParent = activity?.Id,
                    method,
                    route = routeLabel,
                    path = context.Request.Path.Value,
                    statusCode,
                    durationMs = Math.Round(durationSeconds * 1000, 2)
                }, context.RequestAborted);
            }

            _metrics.DecrementHttpRequestsInFlight();
        }
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
}
