namespace TicketService.Api.Contracts.Complaints;

public sealed class ResolveComplaintRequest
{
    public string ResolutionType { get; set; } = string.Empty;
    public string ResolutionNote { get; set; } = string.Empty;
}
