using System.Security.Claims;

namespace AutoRent.Backend.Shared.Auth;

public static class ClaimsPrincipalExtensions
{
    private static readonly string[] UserIdClaimTypes = [ClaimTypes.NameIdentifier, ClaimNames.Subject];
    private static readonly string[] RoleClaimTypes = [ClaimTypes.Role, ClaimNames.Role, ClaimNames.Roles];

    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(ClaimNames.Subject);
    }

    public static string GetRequiredUserId(this ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        return string.IsNullOrWhiteSpace(userId)
            ? throw new UnauthorizedAccessException("Authenticated user id claim is missing.")
            : userId;
    }

    public static Guid? GetUserGuid(this ClaimsPrincipal user)
    {
        return Guid.TryParse(user.GetUserId(), out var userId) ? userId : null;
    }

    public static Guid GetRequiredUserGuid(this ClaimsPrincipal user)
    {
        return user.GetUserGuid()
            ?? throw new UnauthorizedAccessException("Authenticated user id claim is missing or invalid.");
    }

    public static string? GetPreferredUsername(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.Username)
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue(ClaimTypes.Email);
    }

    public static IReadOnlyCollection<string> GetPermissions(this ClaimsPrincipal user)
    {
        return FindValues(user, ClaimNames.Permissions).ToArray();
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        return MatchesAny(user.GetPermissions(), permission);
    }

    public static IReadOnlyCollection<string> GetRoles(this ClaimsPrincipal user)
    {
        return FindValues(user, RoleClaimTypes).ToArray();
    }

    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        return MatchesAny(user.GetRoles(), role);
    }

    public static string? GetSubjectType(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.SubjectType);
    }

    public static string? GetActorType(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.ActorType);
    }

    public static bool IsSubjectType(this ClaimsPrincipal user, string subjectType)
    {
        return Matches(user.GetSubjectType(), subjectType);
    }

    public static bool IsActorType(this ClaimsPrincipal user, string actorType)
    {
        return Matches(user.GetActorType(), actorType);
    }

    public static DateTimeOffset? GetIssuedAtUtc(this ClaimsPrincipal user)
    {
        return ParseJwtTimestamp(user.FindFirstValue(ClaimNames.IssuedAt));
    }

    public static DateTimeOffset? GetExpiresAtUtc(this ClaimsPrincipal user)
    {
        return ParseJwtTimestamp(user.FindFirstValue(ClaimNames.ExpiresAt));
    }

    private static IEnumerable<string> FindValues(ClaimsPrincipal user, params string[] claimTypes)
    {
        return claimTypes
            .SelectMany(user.FindAll)
            .SelectMany(claim => SplitClaimValue(claim.Value))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitClaimValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                yield return item;
            }
        }
    }

    private static bool MatchesAny(IEnumerable<string> values, string expected)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        var normalized = expected.Trim();
        return values.Any(value => Matches(value, normalized) || value == "*");
    }

    private static bool Matches(string? actual, string expected)
    {
        return !string.IsNullOrWhiteSpace(expected)
            && string.Equals(actual, expected.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static DateTimeOffset? ParseJwtTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (long.TryParse(value, out var unixSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }
}
