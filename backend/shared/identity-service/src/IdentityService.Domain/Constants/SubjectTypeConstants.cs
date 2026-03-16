namespace IdentityService.Domain.Constants;

public static class SubjectTypeConstants
{
    public const string User = "user";
    public const string Service = "service";
    public const string ApiKey = "api_key";
    public const string System = "system";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        User,
        Service,
        ApiKey,
        System
    };
}
