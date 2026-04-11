namespace TicketService.Application.Complaints.Commands.AddManagerNote;

public sealed record AddManagerNoteCommand(Guid ComplaintId, Guid ManagerId, string Note);
