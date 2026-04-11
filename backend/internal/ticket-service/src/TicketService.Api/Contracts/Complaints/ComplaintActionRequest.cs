namespace TicketService.Api.Contracts.Complaints;

public sealed record CancelComplaintBookingRequest(string Reason);

public sealed record WaiveComplaintChargeRequest(long ChargeId, string Reason);

public sealed record EscalateComplaintRequest(string Reason);

public sealed record RefundComplaintChargeRequest(long ChargeId, string Reason);
