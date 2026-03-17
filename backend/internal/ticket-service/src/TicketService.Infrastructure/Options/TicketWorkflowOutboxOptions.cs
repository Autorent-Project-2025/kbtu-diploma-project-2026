namespace TicketService.Infrastructure.Options;

public sealed class TicketWorkflowOutboxOptions
{
    public const string SectionName = "TicketWorkflowOutbox";

    public int BatchSize { get; init; } = 20;
    public int PollIntervalSeconds { get; init; } = 5;
    public int LockTimeoutSeconds { get; init; } = 30;
    public int InitialRetryDelaySeconds { get; init; } = 5;
    public int MaxRetryDelaySeconds { get; init; } = 300;
}
