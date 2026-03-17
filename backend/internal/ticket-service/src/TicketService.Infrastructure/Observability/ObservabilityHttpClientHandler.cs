using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TicketService.Infrastructure.Observability;

public sealed class ObservabilityHttpClientHandler : DelegatingHandler
{
    private const string RequestIdHeader = "X-Request-Id";
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ObservabilityHttpClientHandler> _logger;
    private readonly ObservabilityMetrics _metrics;
    private readonly ObservabilityLogWriter _logWriter;

    public ObservabilityHttpClientHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ObservabilityHttpClientHandler> logger,
        ObservabilityMetrics metrics,
        ObservabilityLogWriter logWriter)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _metrics = metrics;
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
        var operation = ObservabilityMetrics.NormalizePathTemplate(request.RequestUri?.AbsolutePath);
        var method = request.Method.Method;

        _metrics.IncrementUpstreamRequestsInFlight();
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var durationSeconds = GetElapsedSeconds(startedAt);
            var outcome = ((int)response.StatusCode).ToString();

            _metrics.RecordUpstreamRequest(target, method, operation, outcome, durationSeconds);
            _logger.LogInformation(
                "Upstream call {Method} {Target}{Operation} completed with {StatusCode} in {DurationMs}ms (requestId: {RequestId}, traceId: {TraceId})",
                method,
                target,
                operation,
                (int)response.StatusCode,
                Math.Round(durationSeconds * 1000, 2),
                requestId,
                activity?.TraceId.ToString());
            await _logWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = "ticket-service",
                @event = "upstream_call_completed",
                requestId,
                traceId = activity?.TraceId.ToString(),
                traceParent = activity?.Id,
                method,
                target,
                operation,
                outcome,
                statusCode = (int)response.StatusCode,
                durationMs = Math.Round(durationSeconds * 1000, 2)
            }, cancellationToken);

            return response;
        }
        catch (Exception exception)
        {
            var durationSeconds = GetElapsedSeconds(startedAt);
            _metrics.RecordUpstreamRequest(target, method, operation, "exception", durationSeconds);
            _logger.LogError(
                exception,
                "Upstream call {Method} {Target}{Operation} failed in {DurationMs}ms (requestId: {RequestId}, traceId: {TraceId})",
                method,
                target,
                operation,
                Math.Round(durationSeconds * 1000, 2),
                requestId,
                activity?.TraceId.ToString());
            await _logWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = "ticket-service",
                @event = "upstream_call_failed",
                requestId,
                traceId = activity?.TraceId.ToString(),
                traceParent = activity?.Id,
                method,
                target,
                operation,
                outcome = "exception",
                durationMs = Math.Round(durationSeconds * 1000, 2),
                error = exception.Message
            }, cancellationToken);
            throw;
        }
        finally
        {
            _metrics.DecrementUpstreamRequestsInFlight();
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

    private static double GetElapsedSeconds(long startedAt)
    {
        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        return elapsed.TotalSeconds;
    }
}
