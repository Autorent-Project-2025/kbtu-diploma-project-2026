namespace TicketService.Application.Complaints.Commands.CancelComplaintBooking;

public sealed record CancelComplaintBookingCommand(
    Guid ComplaintId,
    Guid ManagerId,
    string Reason);
