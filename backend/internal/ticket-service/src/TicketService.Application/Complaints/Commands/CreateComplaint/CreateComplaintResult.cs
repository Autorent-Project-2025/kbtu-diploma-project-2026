using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.CreateComplaint;

public sealed record CreateComplaintResult(ComplaintDto Complaint);
