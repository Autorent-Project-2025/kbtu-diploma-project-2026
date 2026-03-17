using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace TicketService.Infrastructure;

internal static class HttpClientResilienceRegistration
{
    public static HttpClientResilienceOptions GetHttpClientResilienceOptions(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection(HttpClientResilienceOptions.SectionName).Get<HttpClientResilienceOptions>() ?? new();
        Validate(options);
        return options;
    }

    public static IHttpClientBuilder AddConfiguredResilience(
        this IHttpClientBuilder builder,
        HttpClientResilienceOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        builder.AddStandardResilienceHandler(resilience =>
        {
            resilience.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TotalRequestTimeoutSeconds);
            resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.AttemptTimeoutSeconds);
            resilience.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
            resilience.Retry.Delay = TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds);
            resilience.Retry.UseJitter = options.UseJitter;
            resilience.Retry.DisableForUnsafeHttpMethods();
            resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerSamplingDurationSeconds);
            resilience.CircuitBreaker.FailureRatio = options.CircuitBreakerFailureRatio;
            resilience.CircuitBreaker.MinimumThroughput = options.CircuitBreakerMinimumThroughput;
            resilience.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds);
        });

        return builder;
    }

    private static void Validate(HttpClientResilienceOptions options)
    {
        if (options.TotalRequestTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:TotalRequestTimeoutSeconds must be greater than 0.");
        }

        if (options.AttemptTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:AttemptTimeoutSeconds must be greater than 0.");
        }

        if (options.AttemptTimeoutSeconds > options.TotalRequestTimeoutSeconds)
        {
            throw new InvalidOperationException("HttpClientResilience:AttemptTimeoutSeconds must not exceed TotalRequestTimeoutSeconds.");
        }

        if (options.MaxRetryAttempts < 0)
        {
            throw new InvalidOperationException("HttpClientResilience:MaxRetryAttempts must be 0 or greater.");
        }

        if (options.RetryDelayMilliseconds <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:RetryDelayMilliseconds must be greater than 0.");
        }

        if (options.CircuitBreakerSamplingDurationSeconds <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:CircuitBreakerSamplingDurationSeconds must be greater than 0.");
        }

        if (options.CircuitBreakerFailureRatio <= 0 || options.CircuitBreakerFailureRatio > 1)
        {
            throw new InvalidOperationException("HttpClientResilience:CircuitBreakerFailureRatio must be between 0 and 1.");
        }

        if (options.CircuitBreakerMinimumThroughput <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:CircuitBreakerMinimumThroughput must be greater than 0.");
        }

        if (options.CircuitBreakerBreakDurationSeconds <= 0)
        {
            throw new InvalidOperationException("HttpClientResilience:CircuitBreakerBreakDurationSeconds must be greater than 0.");
        }
    }
}

internal sealed class HttpClientResilienceOptions
{
    public const string SectionName = "HttpClientResilience";

    public int TotalRequestTimeoutSeconds { get; init; } = 45;
    public int AttemptTimeoutSeconds { get; init; } = 15;
    public int MaxRetryAttempts { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 250;
    public bool UseJitter { get; init; } = true;
    public int CircuitBreakerSamplingDurationSeconds { get; init; } = 30;
    public double CircuitBreakerFailureRatio { get; init; } = 0.2;
    public int CircuitBreakerMinimumThroughput { get; init; } = 10;
    public int CircuitBreakerBreakDurationSeconds { get; init; } = 15;
}
