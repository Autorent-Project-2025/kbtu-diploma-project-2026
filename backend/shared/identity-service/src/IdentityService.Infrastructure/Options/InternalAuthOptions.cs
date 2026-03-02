namespace IdentityService.Infrastructure.Options;

public sealed class InternalAuthOptions
{
    public const string SectionName = "InternalAuth";

    public string ApiKey { get; init; } = string.Empty;
}
