using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.AddManagerNote;

public sealed record AddManagerNoteResult(ComplaintDto Complaint);
