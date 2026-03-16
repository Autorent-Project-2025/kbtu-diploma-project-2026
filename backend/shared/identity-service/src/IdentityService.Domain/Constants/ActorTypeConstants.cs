namespace IdentityService.Domain.Constants;

public static class ActorTypeConstants
{
    public const string Client = "client";
    public const string Partner = "partner";
    public const string Admin = "admin";
    public const string Internal = "internal";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Client,
        Partner,
        Admin,
        Internal
    };
}
