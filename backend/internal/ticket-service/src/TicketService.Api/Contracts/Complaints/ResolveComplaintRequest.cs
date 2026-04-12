namespace TicketService.Api.Contracts.Complaints;

public sealed class ResolveComplaintRequest
{
    public string? ResolutionType { get; set; }
    public string ResolutionNote { get; set; } = string.Empty;
}
