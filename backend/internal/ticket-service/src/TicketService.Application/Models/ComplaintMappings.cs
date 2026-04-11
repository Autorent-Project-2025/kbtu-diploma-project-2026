using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public static class ComplaintMappings
{
    public static ComplaintDto ToDto(this Complaint complaint)
    {
        return new ComplaintDto(
            complaint.Id,
            complaint.BookingId,
            complaint.ChargeId,
            complaint.ReporterActorType,
            complaint.TargetType,
            complaint.Category,
            complaint.Status,
            complaint.Priority,
            complaint.CreatedByUserId,
            complaint.Subject,
            complaint.Description,
            complaint.AssignedToManagerId,
            complaint.InfoRequestText,
            complaint.InfoRequestAt,
            complaint.InfoRequestBy,
            complaint.InfoResponseText,
            complaint.InfoResponseAt,
            complaint.ManagerNote,
            complaint.ManagerNoteAt,
            complaint.ManagerNoteBy,
            complaint.ResolutionType,
            complaint.ResolutionNote,
            complaint.ResolvedAt,
            complaint.ResolvedBy,
            complaint.RejectionReason,
            complaint.RejectedAt,
            complaint.RejectedBy,
            complaint.SnapshotData,
            complaint.CreatedAt,
            complaint.UpdatedAt,
            complaint.Attachments.Select(a => a.ToDto()).ToArray());
    }

    public static ComplaintAttachmentDto ToDto(this ComplaintAttachment attachment)
    {
        return new ComplaintAttachmentDto(
            attachment.Id,
            attachment.FileName,
            attachment.OriginalFileName,
            attachment.FileType,
            attachment.AttachmentPhase,
            attachment.CreatedAt);
    }
}
