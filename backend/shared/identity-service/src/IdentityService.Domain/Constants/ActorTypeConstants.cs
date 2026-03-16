namespace IdentityService.Domain.Constants;

public static class ActorTypeConstants
{
    public const string Client = "client";
    public const string Partner = "partner";
    public const string Admin = "admin";
    public const string Internal = "internal";

    public static readonly Guid ClientId = new("00000000-0000-0000-0000-000000000201");
    public static readonly Guid PartnerId = new("00000000-0000-0000-0000-000000000202");
    public static readonly Guid AdminId = new("00000000-0000-0000-0000-000000000203");
    public static readonly Guid InternalId = new("00000000-0000-0000-0000-000000000204");

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Client,
        Partner,
        Admin,
        Internal
    };

    private static readonly IReadOnlyDictionary<string, Guid> IdByName =
        new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            [Client] = ClientId,
            [Partner] = PartnerId,
            [Admin] = AdminId,
            [Internal] = InternalId
        };

    private static readonly IReadOnlyDictionary<Guid, string> NameById =
        IdByName.ToDictionary(pair => pair.Value, pair => pair.Key);

    public static Guid GetId(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        if (!IdByName.TryGetValue(normalizedName, out var id))
        {
            throw new ArgumentException($"Actor type '{normalizedName}' is not supported.", nameof(name));
        }

        return id;
    }

    public static string GetName(Guid id)
    {
        if (!NameById.TryGetValue(id, out var name))
        {
            throw new InvalidOperationException($"Actor type id '{id}' is not supported.");
        }

        return name;
    }
}
