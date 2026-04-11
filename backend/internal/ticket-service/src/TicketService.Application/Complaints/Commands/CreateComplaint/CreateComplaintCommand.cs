using TicketService.Application.Models;
using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Commands.CreateComplaint;

public sealed record CreateComplaintCommand(
    Guid CreatedByUserId,
    ReporterActorType ReporterActorType,
    int BookingId,
    ComplaintTargetType TargetType,
    ComplaintCategory Category,
    string Subject,
    string Description,
    IReadOnlyCollection<TicketDocumentFilePayload>? Attachments);
