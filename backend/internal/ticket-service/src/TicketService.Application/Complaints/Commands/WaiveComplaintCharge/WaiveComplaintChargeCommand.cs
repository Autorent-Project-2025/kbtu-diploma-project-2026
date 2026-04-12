namespace TicketService.Application.Complaints.Commands.WaiveComplaintCharge;

public sealed record WaiveComplaintChargeCommand(
    Guid ComplaintId,
    Guid ManagerId,
    long ChargeId,
    string Reason);
