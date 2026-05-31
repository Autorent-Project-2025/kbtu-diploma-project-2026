using Microsoft.AspNetCore.Builder;

namespace AutoRent.Backend.Shared.ProblemDetails;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAutoRentProblemDetails(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AutoRentProblemDetailsMiddleware>();
    }
}
