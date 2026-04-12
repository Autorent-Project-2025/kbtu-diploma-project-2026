using TicketService.Application.Complaints.Services;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetMyComplaintById;

public sealed class GetMyComplaintByIdQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ComplaintChatMigrationService _chatMigration;

    public GetMyComplaintByIdQueryHandler(
        IComplaintRepository complaintRepository,
        ComplaintChatMigrationService chatMigration)
    {
        _complaintRepository = complaintRepository;
        _chatMigration = chatMigration;
    }

    public async Task<GetMyComplaintByIdResult> Handle(
        GetMyComplaintByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (query.ReporterUserId == Guid.Empty)
            throw new ValidationException("Reporter user id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(query.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{query.ComplaintId}' was not found.");

        if (complaint.CreatedByUserId != query.ReporterUserId)
            throw new NotFoundException($"Complaint '{query.ComplaintId}' was not found.");

        // Ensure conversation exists before returning (needed so frontend can fetch it)
        await _chatMigration.EnsureConversationExistsAsync(complaint, cancellationToken);

        return new GetMyComplaintByIdResult(complaint.ToDto());
    }
}
