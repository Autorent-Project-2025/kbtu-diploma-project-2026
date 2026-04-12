using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.TakeComplaint;

public sealed record TakeComplaintResult(ComplaintDto Complaint);
