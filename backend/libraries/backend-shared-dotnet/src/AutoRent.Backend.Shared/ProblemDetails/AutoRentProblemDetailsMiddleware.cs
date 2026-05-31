using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AutoRent.Backend.Shared.ProblemDetails;

public sealed class AutoRentProblemDetailsMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<AutoRentProblemDetailsMiddleware> _logger;

    public AutoRentProblemDetailsMiddleware(
        RequestDelegate next,
        ILogger<AutoRentProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var problemDetails = AutoRentProblemDetailsMapper.Map(exception, context);

            if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing request {Method} {Path} (requestId: {RequestId})",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Handled API exception while processing request {Method} {Path} (requestId: {RequestId})",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);
            }

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails, JsonOptions),
                context.RequestAborted);
        }
    }
}
