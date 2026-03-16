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

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                return false;
            }

            return user.FindAll("permissions")
                .Any(claim => string.Equals(claim.Value, permission.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static string? GetSubjectType(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("subject_type");
        }

        public static string? GetActorType(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("actor_type");
        }

        public static bool IsSubjectType(this ClaimsPrincipal user, string subjectType)
        {
            if (string.IsNullOrWhiteSpace(subjectType))
            {
                return false;
            }

            return string.Equals(user.GetSubjectType(), subjectType.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsActorType(this ClaimsPrincipal user, string actorType)
        {
            if (string.IsNullOrWhiteSpace(actorType))
            {
                return false;
            }

            return string.Equals(user.GetActorType(), actorType.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
