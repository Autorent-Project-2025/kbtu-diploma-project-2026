using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ReviewedByManagerId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    private Ticket() { }

    public Ticket(
        Guid id,
        string fullName,
        string email,
        DateOnly birthDate,
        DateTime createdAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetFullName(fullName);
        SetEmail(email);
        BirthDate = birthDate;
        Status = TicketStatus.Pending;
        CreatedAt = createdAt;
    }

    public void Approve(Guid managerId, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        Status = TicketStatus.Approved;
        DecisionReason = null;
        ReviewedByManagerId = managerId;
        ReviewedAt = reviewedAt;
    }

    public void Reject(Guid managerId, string reason, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(reason));
        }

        Status = TicketStatus.Rejected;
        DecisionReason = reason.Trim();
        ReviewedByManagerId = managerId;
        ReviewedAt = reviewedAt;
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        FullName = fullName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void EnsurePendingStatus()
    {
        if (Status != TicketStatus.Pending)
        {
            throw new InvalidOperationException("Only pending tickets can be reviewed.");
        }
    }

    private static void EnsureManagerId(Guid managerId)
    {
        if (managerId == Guid.Empty)
        {
            throw new ArgumentException("Manager id is required.", nameof(managerId));
        }
    }
}
