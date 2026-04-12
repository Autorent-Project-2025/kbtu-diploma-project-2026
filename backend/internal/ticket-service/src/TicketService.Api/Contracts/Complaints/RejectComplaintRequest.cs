namespace TicketService.Api.Contracts.Complaints;

public sealed class RejectComplaintRequest
{
    public string Reason { get; set; } = string.Empty;
}
