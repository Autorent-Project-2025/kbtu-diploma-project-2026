using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class ComplaintBookingAccessRequest
{
    public Guid Id { get; private set; }
    public Guid ComplaintId { get; private set; }
    public int BookingId { get; private set; }
    public Guid RequestedByManagerId { get; private set; }
    public AccessRequestStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime RequestedAt { get; private set; }

    public Guid? ReviewedBySupermanagerId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? DecisionNote { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private ComplaintBookingAccessRequest() { }

    public static ComplaintBookingAccessRequest Create(
        Guid complaintId,
        int bookingId,
        Guid requestedByManagerId,
        string reason)
    {
        if (complaintId == Guid.Empty)
            throw new ArgumentException("Complaint id is required.", nameof(complaintId));
        if (bookingId <= 0)
            throw new ArgumentException("Booking id must be positive.", nameof(bookingId));
        if (requestedByManagerId == Guid.Empty)
            throw new ArgumentException("Manager id is required.", nameof(requestedByManagerId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));
        if (reason.Length > 2000)
            throw new ArgumentException("Reason must not exceed 2000 characters.", nameof(reason));

        return new ComplaintBookingAccessRequest
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
            BookingId = bookingId,
            RequestedByManagerId = requestedByManagerId,
            Status = AccessRequestStatus.Pending,
            Reason = reason.Trim(),
            RequestedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid supermanagerId, string? decisionNote, DateTime expiresAt)
    {
        if (Status != AccessRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");
        if (supermanagerId == Guid.Empty)
            throw new ArgumentException("Supermanager id is required.", nameof(supermanagerId));
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiry time must be in the future.", nameof(expiresAt));

        Status = AccessRequestStatus.Approved;
        ReviewedBySupermanagerId = supermanagerId;
        ReviewedAt = DateTime.UtcNow;
        DecisionNote = decisionNote?.Trim();
        ExpiresAt = expiresAt;
    }

    public void Reject(Guid supermanagerId, string? decisionNote)
    {
        if (Status != AccessRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");
        if (supermanagerId == Guid.Empty)
            throw new ArgumentException("Supermanager id is required.", nameof(supermanagerId));

        Status = AccessRequestStatus.Rejected;
        ReviewedBySupermanagerId = supermanagerId;
        ReviewedAt = DateTime.UtcNow;
        DecisionNote = decisionNote?.Trim();
    }

    public void Revoke(Guid supermanagerId)
    {
        if (Status != AccessRequestStatus.Approved)
            throw new InvalidOperationException("Only approved requests can be revoked.");

        Status = AccessRequestStatus.Revoked;
        ReviewedBySupermanagerId = supermanagerId;
        ReviewedAt = DateTime.UtcNow;
    }

    public bool IsActiveGrant()
    {
        return Status == AccessRequestStatus.Approved
               && ExpiresAt.HasValue
               && ExpiresAt.Value > DateTime.UtcNow;
    }
}
