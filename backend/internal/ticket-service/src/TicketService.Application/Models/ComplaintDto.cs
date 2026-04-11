using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Models;

public sealed record ComplaintDto(
    Guid Id,
    int BookingId,
    long? ChargeId,
    ReporterActorType ReporterActorType,
    ComplaintTargetType TargetType,
    ComplaintCategory Category,
    ComplaintStatus Status,
    ComplaintPriority Priority,
    Guid CreatedByUserId,
    string Subject,
    string Description,
    Guid? AssignedToManagerId,
    string? InfoRequestText,
    DateTime? InfoRequestAt,
    Guid? InfoRequestBy,
    string? InfoResponseText,
    DateTime? InfoResponseAt,
    string? ManagerNote,
    DateTime? ManagerNoteAt,
    Guid? ManagerNoteBy,
    ComplaintResolutionType? ResolutionType,
    string? ResolutionNote,
    DateTime? ResolvedAt,
    Guid? ResolvedBy,
    string? RejectionReason,
    DateTime? RejectedAt,
    Guid? RejectedBy,
    bool IsEscalated,
    DateTime? EscalatedAt,
    Guid? EscalatedBy,
    string? EscalationReason,
    BookingSnapshotData SnapshotData,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ComplaintAttachmentDto> Attachments);

public sealed record ComplaintAttachmentDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string FileType,
    AttachmentPhase AttachmentPhase,
    DateTime CreatedAt);
