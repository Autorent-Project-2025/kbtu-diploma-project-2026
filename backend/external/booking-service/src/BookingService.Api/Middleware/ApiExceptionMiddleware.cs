using BookingService.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Text.Json;

namespace BookingService.Api.Middleware
{
    public class ApiExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (statusCode, title) = MapException(ex);

                if (statusCode >= StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(ex, "Unhandled exception while processing request {Method} {Path}.", context.Request.Method, context.Request.Path);
                }
                else
                {
                    _logger.LogWarning(ex, "Request validation/business error for {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, ex.Message);
                }

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                if (ex is BookingCompletionAiRejectedException aiRejected)
                {
                    // Enriched problem payload so the client UI can mark
                    // the exact photo slots that failed and display AI
                    // rejection reasons. Keep filenames and technical
                    // step numbers out of the user-facing fields where
                    // possible — UI may still render them for debugging.
                    var aiProblem = new AiRejectedProblemDetails
                    {
                        Status = statusCode,
                        Title = title,
                        Detail = ex.Message,
                        Instance = context.Request.Path,
                        ValidPhotosCount = aiRejected.ValidPhotosCount,
                        RejectedPhotos = aiRejected.RejectedPhotos
                            .Select(photo => new AiRejectedPhotoItem
                            {
                                Slot = photo.Slot,
                                FileName = photo.FileName,
                                Reason = photo.Reason,
                                Details = photo.Details,
                            })
                            .ToList(),
                    };

                    await JsonSerializer.SerializeAsync(context.Response.Body, aiProblem, JsonOptions);
                    return;
                }

                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = statusCode >= StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : ex.Message,
                    Instance = context.Request.Path
                };

                await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions);
            }
        }

        private static (int StatusCode, string Title) MapException(Exception ex)
        {
            return ex switch
            {
                BookingCompletionAiRejectedException => (StatusCodes.Status400BadRequest, "Completion Photos Rejected"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                SecurityException => (StatusCodes.Status403Forbidden, "Forbidden"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };
        }

        private sealed class AiRejectedProblemDetails : ProblemDetails
        {
            public int ValidPhotosCount { get; set; }
            public IList<AiRejectedPhotoItem> RejectedPhotos { get; set; } = new List<AiRejectedPhotoItem>();
        }

        private sealed class AiRejectedPhotoItem
        {
            public string? Slot { get; set; }
            public string FileName { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public IReadOnlyList<string> Details { get; set; } = Array.Empty<string>();
        }
    }
}
