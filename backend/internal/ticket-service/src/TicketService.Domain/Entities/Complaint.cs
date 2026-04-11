using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class Complaint
{
    private readonly List<ComplaintAttachment> _attachments = [];

    public Guid Id { get; private set; }
    public int BookingId { get; private set; }
    public long? ChargeId { get; private set; }

    public ReporterActorType ReporterActorType { get; private set; }
    public ComplaintTargetType TargetType { get; private set; }
    public ComplaintCategory Category { get; private set; }
    public ComplaintStatus Status { get; private set; }
    public ComplaintPriority Priority { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid? AssignedToManagerId { get; private set; }

    public string? InfoRequestText { get; private set; }
    public DateTime? InfoRequestAt { get; private set; }
    public Guid? InfoRequestBy { get; private set; }
    public string? InfoResponseText { get; private set; }
    public DateTime? InfoResponseAt { get; private set; }

    public string? ManagerNote { get; private set; }
    public DateTime? ManagerNoteAt { get; private set; }
    public Guid? ManagerNoteBy { get; private set; }

    public ComplaintResolutionType? ResolutionType { get; private set; }
    public string? ResolutionNote { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedBy { get; private set; }

    public string? RejectionReason { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }

    public BookingSnapshotData SnapshotData { get; private set; } = new();

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ComplaintAttachment> Attachments => _attachments.AsReadOnly();

    private Complaint() { }

    public static Complaint Create(
        int bookingId,
        Guid createdByUserId,
        ReporterActorType reporterActorType,
        ComplaintTargetType targetType,
        ComplaintCategory category,
        string subject,
        string description,
        BookingSnapshotData snapshotData)
    {
        if (bookingId <= 0)
            throw new ArgumentException("Booking id must be greater than zero.", nameof(bookingId));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Creator user id is required.", nameof(createdByUserId));

        ValidateReporterTargetCombination(reporterActorType, targetType);
        ValidateCategoryForReporter(reporterActorType, category);

        var normalizedSubject = NormalizeRequired(subject, nameof(subject), 200);
        var normalizedDescription = NormalizeRequired(description, nameof(description), 4000);

        var now = DateTime.UtcNow;
        return new Complaint
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            CreatedByUserId = createdByUserId,
            ReporterActorType = reporterActorType,
            TargetType = targetType,
            Category = category,
            Status = ComplaintStatus.New,
            Priority = category == ComplaintCategory.SafetyIssue
                ? ComplaintPriority.Urgent
                : ComplaintPriority.Normal,
            Subject = normalizedSubject,
            Description = normalizedDescription,
            SnapshotData = snapshotData ?? throw new ArgumentNullException(nameof(snapshotData)),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void AddAttachment(ComplaintAttachment attachment)
    {
        if (attachment is null)
            throw new ArgumentNullException(nameof(attachment));

        _attachments.Add(attachment);
    }

    public void Take(Guid managerId)
    {
        EnsureManagerId(managerId);
        EnsureStatus(ComplaintStatus.New, "Only new complaints can be taken.");

        Status = ComplaintStatus.InReview;
        AssignedToManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RequestInfo(Guid managerId, string message)
    {
        EnsureManagerId(managerId);
        EnsureAssignedManager(managerId);
        EnsureStatus(ComplaintStatus.InReview, "Info can only be requested when complaint is in review.");

        if (InfoRequestText is not null)
            throw new InvalidOperationException("Info has already been requested for this complaint.");

        var normalizedMessage = NormalizeRequired(message, nameof(message), 4000);

        Status = ComplaintStatus.AwaitingResponse;
        InfoRequestText = normalizedMessage;
        InfoRequestAt = DateTime.UtcNow;
        InfoRequestBy = managerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RespondToInfoRequest(Guid reporterUserId, string message)
    {
        if (reporterUserId == Guid.Empty)
            throw new ArgumentException("Reporter user id is required.", nameof(reporterUserId));
        if (reporterUserId != CreatedByUserId)
            throw new InvalidOperationException("Only the complaint reporter can respond to info requests.");

        EnsureStatus(ComplaintStatus.AwaitingResponse, "Response can only be submitted when info is requested.");

        if (InfoResponseText is not null)
            throw new InvalidOperationException("Info response has already been submitted.");

        var normalizedMessage = NormalizeRequired(message, nameof(message), 4000);

        Status = ComplaintStatus.InReview;
        InfoResponseText = normalizedMessage;
        InfoResponseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddManagerNote(Guid managerId, string note)
    {
        EnsureManagerId(managerId);
        EnsureAssignedManager(managerId);
        EnsureNotTerminal();

        var normalizedNote = NormalizeRequired(note, nameof(note), 4000);

        ManagerNote = normalizedNote;
        ManagerNoteAt = DateTime.UtcNow;
        ManagerNoteBy = managerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resolve(Guid managerId, ComplaintResolutionType resolutionType, string resolutionNote)
    {
        EnsureManagerId(managerId);
        EnsureAssignedManager(managerId);
        EnsureStatus(ComplaintStatus.InReview, "Only complaints in review can be resolved.");

        if (!Enum.IsDefined(resolutionType))
            throw new ArgumentException("Resolution type is invalid.", nameof(resolutionType));

        var normalizedNote = NormalizeRequired(resolutionNote, nameof(resolutionNote), 4000);
        var now = DateTime.UtcNow;

        Status = ComplaintStatus.Resolved;
        ResolutionType = resolutionType;
        ResolutionNote = normalizedNote;
        ResolvedAt = now;
        ResolvedBy = managerId;
        UpdatedAt = now;
    }

    public void Reject(Guid managerId, string reason)
    {
        EnsureManagerId(managerId);
        EnsureAssignedManager(managerId);
        EnsureStatus(ComplaintStatus.InReview, "Only complaints in review can be rejected.");

        var normalizedReason = NormalizeRequired(reason, nameof(reason), 4000);
        var now = DateTime.UtcNow;

        Status = ComplaintStatus.Rejected;
        RejectionReason = normalizedReason;
        RejectedAt = now;
        RejectedBy = managerId;
        UpdatedAt = now;
    }

    private static void ValidateReporterTargetCombination(
        ReporterActorType reporter,
        ComplaintTargetType target)
    {
        var isValid = (reporter, target) switch
        {
            (ReporterActorType.Client, ComplaintTargetType.Partner) => true,
            (ReporterActorType.Partner, ComplaintTargetType.Client) => true,
            _ => false
        };

        if (!isValid)
            throw new ArgumentException($"Reporter '{reporter}' cannot file a complaint targeting '{target}'.");
    }

    private static void ValidateCategoryForReporter(
        ReporterActorType reporter,
        ComplaintCategory category)
    {
        var isValid = reporter switch
        {
            ReporterActorType.Client => category is ComplaintCategory.CarCondition
                or ComplaintCategory.LateHandover
                or ComplaintCategory.ServiceQuality
                or ComplaintCategory.SafetyIssue
                or ComplaintCategory.Other,
            ReporterActorType.Partner => category is ComplaintCategory.SafetyIssue
                or ComplaintCategory.ClientMisbehavior
                or ComplaintCategory.Other,
            _ => false
        };

        if (!isValid)
            throw new ArgumentException($"Category '{category}' is not allowed for reporter type '{reporter}'.");
    }

    private void EnsureStatus(ComplaintStatus expected, string errorMessage)
    {
        if (Status != expected)
            throw new InvalidOperationException(errorMessage);
    }

    private void EnsureNotTerminal()
    {
        if (Status is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
            throw new InvalidOperationException("Cannot modify a resolved or rejected complaint.");
    }

    private void EnsureAssignedManager(Guid managerId)
    {
        if (AssignedToManagerId is null || AssignedToManagerId != managerId)
            throw new InvalidOperationException("Only the assigned manager can perform this action.");
    }

    private static void EnsureManagerId(Guid managerId)
    {
        if (managerId == Guid.Empty)
            throw new ArgumentException("Manager id is required.", nameof(managerId));
    }

    private static string NormalizeRequired(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} is required.", paramName);

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);

        return normalized;
    }
}
