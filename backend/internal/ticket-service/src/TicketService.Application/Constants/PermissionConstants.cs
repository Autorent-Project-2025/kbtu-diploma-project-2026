namespace TicketService.Application.Constants;

public static class PermissionConstants
{
    public const string TicketView = "Ticket.View";
    public const string TicketApprove = "Ticket.Approve";
    public const string TicketReject = "Ticket.Reject";
    public const string TicketViewAll = "Ticket.ViewAll";

    public const string ComplaintView = "Complaint.View";
    public const string ComplaintReview = "Complaint.Review";
    public const string ComplaintResolve = "Complaint.Resolve";

    public const string AccessRequestReview = "AccessRequest.Review";

    public const string ComplaintActionCancelBooking = "Complaint.Action.CancelBooking";
    public const string ComplaintActionWaiveCharge = "Complaint.Action.WaiveCharge";
    public const string ComplaintActionEscalate = "Complaint.Action.Escalate";
    public const string ComplaintActionRefundCharge = "Complaint.Action.RefundCharge";

    public const string BookingView = "Booking.View";
    public const string BookingUpdate = "Booking.Update";
}
