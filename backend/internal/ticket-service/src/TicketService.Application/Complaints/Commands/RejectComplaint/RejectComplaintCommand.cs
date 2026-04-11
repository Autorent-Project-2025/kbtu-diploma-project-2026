namespace TicketService.Application.Complaints.Commands.RejectComplaint;

public sealed record RejectComplaintCommand(Guid ComplaintId, Guid ManagerId, string Reason);
