namespace TicketService.Application.Complaints.Commands.RefundComplaintCharge;

public sealed record RefundComplaintChargeCommand(
    Guid ComplaintId,
    Guid ManagerId,
    long ChargeId,
    string Reason);
