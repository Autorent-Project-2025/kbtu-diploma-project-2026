using IdentityService.Application.Exceptions;
using IdentityService.Domain.Constants;

namespace IdentityService.Application.Utils;

public static class UserTypeValidator
{
    public static string ResolveSubjectTypeOrDefault(string? subjectType)
    {
        return subjectType is null
            ? SubjectTypeConstants.User
            : ValidateSubjectType(subjectType);
    }

    public static string ResolveActorTypeOrDefault(string? actorType)
    {
        return actorType is null
            ? ActorTypeConstants.Client
            : ValidateActorType(actorType);
    }

    public static string ValidateSubjectType(string subjectType)
    {
        return NormalizeKnownType(
            subjectType,
            "Subject type",
            SubjectTypeConstants.All);
    }

    public static string ValidateActorType(string actorType)
    {
        return NormalizeKnownType(
            actorType,
            "Actor type",
            ActorTypeConstants.All);
    }

    private static string NormalizeKnownType(
        string value,
        string label,
        HashSet<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{label} is required.");
        }

        var normalizedValue = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalizedValue))
        {
            throw new ValidationException($"{label} '{normalizedValue}' is not supported.");
        }

        return normalizedValue;
    }
}
