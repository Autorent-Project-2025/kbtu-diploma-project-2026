using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace IdentityService.Api.Observability;

public sealed class ObservabilityMetrics
{
    private readonly ConcurrentDictionary<HttpRequestMetricKey, MetricAggregate> _httpRequests = new();
    private long _httpRequestsInFlight;

    public void IncrementHttpRequestsInFlight() => Interlocked.Increment(ref _httpRequestsInFlight);

    public void DecrementHttpRequestsInFlight() => Interlocked.Decrement(ref _httpRequestsInFlight);

    public void RecordHttpRequest(string method, string route, int statusCode, double durationSeconds)
    {
        var key = new HttpRequestMetricKey(
            NormalizeLabelValue(method, "UNKNOWN"),
            NormalizePathTemplate(route),
            statusCode.ToString(CultureInfo.InvariantCulture));

        _httpRequests.AddOrUpdate(
            key,
            static (_, value) => value,
            static (_, current, value) => current.Add(value),
            new MetricAggregate(1, durationSeconds));
    }

    public string RenderPrometheus()
    {
        var builder = new StringBuilder();

        builder.AppendLine("# HELP autorent_identity_service_http_requests_in_flight Current number of HTTP requests being processed by identity-service.");
        builder.AppendLine("# TYPE autorent_identity_service_http_requests_in_flight gauge");
        builder.Append("autorent_identity_service_http_requests_in_flight ");
        builder.AppendLine(_httpRequestsInFlight.ToString(CultureInfo.InvariantCulture));

        builder.AppendLine("# HELP autorent_identity_service_http_requests_total Total HTTP requests processed by identity-service.");
        builder.AppendLine("# TYPE autorent_identity_service_http_requests_total counter");
        foreach (var (key, aggregate) in _httpRequests
                     .OrderBy(static entry => entry.Key.Method)
                     .ThenBy(static entry => entry.Key.Route)
                     .ThenBy(static entry => entry.Key.StatusCode))
        {
            builder.Append("autorent_identity_service_http_requests_total");
            AppendLabels(builder, ("method", key.Method), ("route", key.Route), ("status", key.StatusCode));
            builder.Append(' ');
            builder.AppendLine(aggregate.Count.ToString(CultureInfo.InvariantCulture));
        }

        builder.AppendLine("# HELP autorent_identity_service_http_request_duration_seconds Request duration for identity-service HTTP endpoints.");
        builder.AppendLine("# TYPE autorent_identity_service_http_request_duration_seconds summary");
        foreach (var (key, aggregate) in _httpRequests
                     .OrderBy(static entry => entry.Key.Method)
                     .ThenBy(static entry => entry.Key.Route)
                     .ThenBy(static entry => entry.Key.StatusCode))
        {
            builder.Append("autorent_identity_service_http_request_duration_seconds_count");
            AppendLabels(builder, ("method", key.Method), ("route", key.Route), ("status", key.StatusCode));
            builder.Append(' ');
            builder.AppendLine(aggregate.Count.ToString(CultureInfo.InvariantCulture));

            builder.Append("autorent_identity_service_http_request_duration_seconds_sum");
            AppendLabels(builder, ("method", key.Method), ("route", key.Route), ("status", key.StatusCode));
            builder.Append(' ');
            builder.AppendLine(aggregate.DurationSeconds.ToString("0.000000", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    public static string NormalizePathTemplate(string? rawPath)
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

        if (Guid.TryParse(segment, out _))
        {
            return "{id}";
        }

        if (long.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return "{id}";
        }

        return segment.Trim().ToLowerInvariant();
    }

    private static string NormalizeLabelValue(string? rawValue, string fallback)
    {
        var trimmed = rawValue?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }

    private static void AppendLabels(StringBuilder builder, params (string Name, string Value)[] labels)
    {
        builder.Append('{');
        for (var index = 0; index < labels.Length; index++)
        {
            var (name, value) = labels[index];
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.Append(name);
            builder.Append("=\"");
            builder.Append(EscapeLabelValue(value));
            builder.Append('"');
        }

        builder.Append('}');
    }

    private static string EscapeLabelValue(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }

    private readonly record struct HttpRequestMetricKey(string Method, string Route, string StatusCode);

    private readonly record struct MetricAggregate(long Count, double DurationSeconds)
    {
        public MetricAggregate Add(MetricAggregate other) => new(Count + other.Count, DurationSeconds + other.DurationSeconds);
    }
}
