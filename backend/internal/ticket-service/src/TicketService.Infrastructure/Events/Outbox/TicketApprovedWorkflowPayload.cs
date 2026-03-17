namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class TicketApprovedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public TicketApprovedWorkflowStep CurrentStep { get; set; }
    public Guid? ProvisionedUserId { get; set; }
    public string? LoginEmail { get; set; }
    public string? ActivationToken { get; set; }
}

internal enum TicketApprovedWorkflowStep
{
    ProvisionIdentity = 1,
    ProvisionProfile = 2,
    PublishApprovedEmail = 3,
    PublishPartnerCarProvision = 4,
    PublishPartnerCarApprovedEmail = 5,
    Completed = 99
}
