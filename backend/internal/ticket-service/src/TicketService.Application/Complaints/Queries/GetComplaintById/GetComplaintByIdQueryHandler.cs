using TicketService.Application.Complaints.Services;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetComplaintById;

public sealed class GetComplaintByIdQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ComplaintChatMigrationService _chatMigration;

    public GetComplaintByIdQueryHandler(
        IComplaintRepository complaintRepository,
        ComplaintChatMigrationService chatMigration)
    {
        _complaintRepository = complaintRepository;
        _chatMigration = chatMigration;
    }

    public async Task<GetComplaintByIdResult> Handle(
        GetComplaintByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(query.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{query.ComplaintId}' was not found.");

        // Ensure conversation exists before returning (needed so frontend can fetch it)
        await _chatMigration.EnsureConversationExistsAsync(complaint, cancellationToken);

        return new GetComplaintByIdResult(complaint.ToDto());
    }
}
