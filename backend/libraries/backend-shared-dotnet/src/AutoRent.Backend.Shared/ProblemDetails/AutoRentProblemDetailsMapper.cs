using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoRent.Backend.Shared.ProblemDetails;

public static class AutoRentProblemDetailsMapper
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails Map(Exception exception, HttpContext context)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["errorType"] = exception.GetType().Name;

        return problemDetails;
    }

    public static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException or ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            SecurityException => StatusCodes.Status403Forbidden,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => GetStatusCodeByTypeName(exception.GetType().Name)
        };
    }

    private static int GetStatusCodeByTypeName(string exceptionTypeName)
    {
        return exceptionTypeName switch
        {
            "ValidationException" => StatusCodes.Status400BadRequest,
            "UnauthorizedException" => StatusCodes.Status401Unauthorized,
            "ForbiddenException" or "SecurityException" => StatusCodes.Status403Forbidden,
            "NotFoundException" => StatusCodes.Status404NotFound,
            "ConflictException" or "DbUpdateConcurrencyException" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };
    }
}
