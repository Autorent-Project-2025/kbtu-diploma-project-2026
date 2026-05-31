using Microsoft.AspNetCore.Http;

namespace AutoRent.Backend.Shared.Results;

public static class ResultProblemDetailsExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(
        this Result result,
        HttpContext context,
        int defaultStatusCode = StatusCodes.Status400BadRequest)
    {
        if (result.Succeeded || result.Error is null)
        {
            throw new InvalidOperationException("Cannot create ProblemDetails from a successful result.");
        }

        var statusCode = result.Error.StatusCode ?? defaultStatusCode;
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = result.Error.Message,
            Type = result.Error.Code,
            Instance = context.Request.Path
        };
    }

    public static IResult ToHttpResult(
        this Result result,
        HttpContext context,
        Func<IResult>? onSuccess = null)
    {
        return result.Succeeded
            ? onSuccess?.Invoke() ?? Microsoft.AspNetCore.Http.Results.NoContent()
            : Microsoft.AspNetCore.Http.Results.Problem(result.ToProblemDetails(context));
    }

    public static IResult ToHttpResult<T>(
        this Result<T> result,
        HttpContext context,
        Func<T, IResult>? onSuccess = null)
    {
        if (result.Succeeded)
        {
            return onSuccess?.Invoke(result.Value!)
                ?? Microsoft.AspNetCore.Http.Results.Ok(result.Value);
        }

        return Microsoft.AspNetCore.Http.Results.Problem(result.ToProblemDetails(context));
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
            StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
            _ => "Request Failed"
        };
    }
}
