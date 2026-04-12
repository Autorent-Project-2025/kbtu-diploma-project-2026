namespace TicketService.Application.Complaints.Commands.TakeComplaint;

public sealed record TakeComplaintCommand(Guid ComplaintId, Guid ManagerId);
