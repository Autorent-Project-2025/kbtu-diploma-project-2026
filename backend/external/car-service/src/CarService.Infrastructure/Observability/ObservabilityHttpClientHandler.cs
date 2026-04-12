using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarService.Infrastructure.Observability;

public sealed class ObservabilityHttpClientHandler : DelegatingHandler
{
    private const string RequestIdHeader = "X-Request-Id";
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";
    private const string ServiceName = "car-service";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ObservabilityHttpClientHandler> _logger;
    private readonly ObservabilityLogWriter _logWriter;

    public ObservabilityHttpClientHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ObservabilityHttpClientHandler> logger,
        ObservabilityLogWriter logWriter)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _logWriter = logWriter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestId = ResolveRequestId();
        if (!request.Headers.Contains(RequestIdHeader))
        {
            request.Headers.TryAddWithoutValidation(RequestIdHeader, requestId);
        }

        var activity = Activity.Current;
        if (activity is not null)
        {
            request.Headers.Remove(TraceParentHeader);
            request.Headers.TryAddWithoutValidation(TraceParentHeader, activity.Id);

            var traceState = activity.TraceStateString;
            if (!string.IsNullOrWhiteSpace(traceState))
            {
                request.Headers.Remove(TraceStateHeader);
                request.Headers.TryAddWithoutValidation(TraceStateHeader, traceState);
            }
        }

        var target = request.RequestUri?.Host ?? "unknown";
        var operation = NormalizePathTemplate(request.RequestUri?.AbsolutePath);
        var method = request.Method.Method;
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var durationMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2);
            var statusCode = (int)response.StatusCode;

            _logger.LogInformation(
                "Upstream call {Method} {Target}{Operation} completed with {StatusCode} in {DurationMs}ms (requestId: {RequestId}, traceId: {TraceId})",
                method,
                target,
                operation,
                statusCode,
                durationMs,
                requestId,
                activity?.TraceId.ToString());

            await _logWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = ServiceName,
                level = "Information",
                @event = "upstream_call_completed",
                requestId,
                traceId = activity?.TraceId.ToString(),
                traceParent = activity?.Id,
                method,
                target,
                operation,
                outcome = statusCode.ToString(),
                statusCode,
                durationMs
            }, cancellationToken);

            return response;
        }
        catch (Exception exception)
        {
            var durationMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2);

            _logger.LogError(
                exception,
                "Upstream call {Method} {Target}{Operation} failed in {DurationMs}ms (requestId: {RequestId}, traceId: {TraceId})",
                method,
                target,
                operation,
                durationMs,
                requestId,
                activity?.TraceId.ToString());

            await _logWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = ServiceName,
                level = "Error",
                @event = "upstream_call_failed",
                requestId,
                traceId = activity?.TraceId.ToString(),
                traceParent = activity?.Id,
                method,
                target,
                operation,
                outcome = "exception",
                durationMs,
                error = exception.Message
            }, cancellationToken);

            throw;
        }
    }

    private string ResolveRequestId()
    {
        var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            return requestId;
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
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
