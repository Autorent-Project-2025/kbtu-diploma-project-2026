using System.Security.Claims;

namespace PartnerService.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
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
