namespace TicketService.Application.Complaints.Commands.EscalateComplaint;

public sealed record EscalateComplaintCommand(
    Guid ComplaintId,
    Guid ManagerId,
    string Reason);
