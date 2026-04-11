using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RejectComplaint;

public sealed record RejectComplaintResult(ComplaintDto Complaint);
