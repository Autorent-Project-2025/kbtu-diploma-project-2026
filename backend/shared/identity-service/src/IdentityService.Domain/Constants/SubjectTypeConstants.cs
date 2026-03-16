namespace IdentityService.Domain.Constants;

public static class SubjectTypeConstants
{
    public const string User = "user";
    public const string Service = "service";
    public const string ApiKey = "api_key";
    public const string System = "system";

    public static readonly Guid UserId = new("00000000-0000-0000-0000-000000000101");
    public static readonly Guid ServiceId = new("00000000-0000-0000-0000-000000000102");
    public static readonly Guid ApiKeyId = new("00000000-0000-0000-0000-000000000103");
    public static readonly Guid SystemId = new("00000000-0000-0000-0000-000000000104");

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        User,
        Service,
        ApiKey,
        System
    };

    private static readonly IReadOnlyDictionary<string, Guid> IdByName =
        new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            [User] = UserId,
            [Service] = ServiceId,
            [ApiKey] = ApiKeyId,
            [System] = SystemId
        };

    private static readonly IReadOnlyDictionary<Guid, string> NameById =
        IdByName.ToDictionary(pair => pair.Value, pair => pair.Key);

    public static Guid GetId(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        if (!IdByName.TryGetValue(normalizedName, out var id))
        {
            throw new ArgumentException($"Subject type '{normalizedName}' is not supported.", nameof(name));
        }

        return id;
    }

    public static string GetName(Guid id)
    {
        if (!NameById.TryGetValue(id, out var name))
        {
            throw new InvalidOperationException($"Subject type id '{id}' is not supported.");
        }

        return name;
    }
}
