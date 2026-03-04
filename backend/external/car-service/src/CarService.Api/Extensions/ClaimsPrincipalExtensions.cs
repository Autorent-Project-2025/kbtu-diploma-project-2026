using System.Security.Claims;

namespace CarService.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetRequiredUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Authenticated user id claim is required.");
            }

            return userId;
        }

        public static Guid GetRequiredUserGuid(this ClaimsPrincipal user)
        {
            var userId = user.GetRequiredUserId();
            if (!Guid.TryParse(userId, out var parsed))
            {
                throw new UnauthorizedAccessException("Authenticated user id must be UUID.");
            }

            return parsed;
        }

        public static string GetPreferredUserName(this ClaimsPrincipal user)
        {
            var username = user.FindFirstValue("username")
                ?? user.Identity?.Name
                ?? user.GetRequiredUserId();

            return username;
        }
    }
}
