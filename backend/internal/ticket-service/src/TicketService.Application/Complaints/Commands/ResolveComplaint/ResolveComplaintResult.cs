using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.ResolveComplaint;

public sealed record ResolveComplaintResult(ComplaintDto Complaint);
