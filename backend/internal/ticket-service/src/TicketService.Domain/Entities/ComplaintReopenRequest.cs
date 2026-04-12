using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class ComplaintReopenRequest
{
    public Guid Id { get; private set; }
    public Guid ComplaintId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReopenRequestStatus Status { get; private set; }
    public Guid? ReviewedByManagerId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? DecisionNote { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ComplaintReopenRequest() { }

    public static ComplaintReopenRequest Create(
        Guid complaintId,
        Guid requestedByUserId,
        string reason)
    {
        if (complaintId == Guid.Empty)
            throw new ArgumentException("Complaint id is required.", nameof(complaintId));
        if (requestedByUserId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(requestedByUserId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        var normalized = reason.Trim();
        if (normalized.Length > 4000)
            throw new ArgumentException("Reason must not exceed 4000 characters.", nameof(reason));

        return new ComplaintReopenRequest
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
            RequestedByUserId = requestedByUserId,
            Reason = normalized,
            Status = ReopenRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid managerId, string? note)
    {
        EnsurePending();
        if (managerId == Guid.Empty)
            throw new ArgumentException("Manager id is required.", nameof(managerId));

        Status = ReopenRequestStatus.Approved;
        ReviewedByManagerId = managerId;
        ReviewedAt = DateTime.UtcNow;
        DecisionNote = note?.Trim();
    }

    public void Reject(Guid managerId, string? note)
    {
        EnsurePending();
        if (managerId == Guid.Empty)
            throw new ArgumentException("Manager id is required.", nameof(managerId));

        Status = ReopenRequestStatus.Rejected;
        ReviewedByManagerId = managerId;
        ReviewedAt = DateTime.UtcNow;
        DecisionNote = note?.Trim();
    }

    private void EnsurePending()
    {
        if (Status != ReopenRequestStatus.Pending)
            throw new InvalidOperationException("Only pending reopen requests can be reviewed.");
    }
}
