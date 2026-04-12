namespace TicketService.Domain.Entities;

public sealed class ComplaintActionLog
{
    public Guid Id { get; private set; }
    public Guid ComplaintId { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public Guid PerformedBy { get; private set; }
    public string? Comment { get; private set; }
    public string? TargetEntityType { get; private set; }
    public string? TargetEntityId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ComplaintActionLog() { }

    public static ComplaintActionLog Create(
        Guid complaintId,
        string actionType,
        Guid performedBy,
        string? comment = null,
        string? targetEntityType = null,
        string? targetEntityId = null)
    {
        if (complaintId == Guid.Empty)
            throw new ArgumentException("Complaint id is required.", nameof(complaintId));
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("Action type is required.", nameof(actionType));
        if (performedBy == Guid.Empty)
            throw new ArgumentException("Performer id is required.", nameof(performedBy));

        return new ComplaintActionLog
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
            ActionType = actionType.Trim(),
            PerformedBy = performedBy,
            Comment = comment?.Trim(),
            TargetEntityType = targetEntityType?.Trim(),
            TargetEntityId = targetEntityId?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
