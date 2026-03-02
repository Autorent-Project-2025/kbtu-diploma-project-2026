namespace TicketService.Infrastructure.Options;

public sealed class ActivationOptions
{
    public const string SectionName = "Activation";

    public string SetPasswordBaseUrl { get; init; } = string.Empty;
}
