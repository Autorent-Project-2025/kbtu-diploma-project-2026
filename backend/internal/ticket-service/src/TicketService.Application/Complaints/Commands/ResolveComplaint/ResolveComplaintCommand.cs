using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Commands.ResolveComplaint;

public sealed record ResolveComplaintCommand(
    Guid ComplaintId,
    Guid ManagerId,
    ComplaintResolutionType ResolutionType,
    string ResolutionNote);
