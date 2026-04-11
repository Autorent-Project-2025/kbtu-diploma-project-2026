using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.ApproveReopenRequest;

public sealed class ApproveReopenRequestCommandHandler
{
    private readonly IReopenRequestRepository _reopenRequestRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public ApproveReopenRequestCommandHandler(
        IReopenRequestRepository reopenRequestRepository,
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _reopenRequestRepository = reopenRequestRepository;
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<ApproveReopenRequestResult> Handle(
        ApproveReopenRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ReopenRequestId == Guid.Empty)
            throw new ValidationException("Reopen request id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var reopenRequest = await _reopenRequestRepository.GetByIdAsync(
            command.ReopenRequestId, cancellationToken);
        if (reopenRequest is null)
            throw new NotFoundException($"Reopen request '{command.ReopenRequestId}' was not found.");

        var complaint = await _complaintRepository.GetByIdAsync(
            reopenRequest.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{reopenRequest.ComplaintId}' was not found.");

        reopenRequest.Approve(command.ManagerId, command.Note);
        complaint.Reopen(command.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reopen chat conversation
        var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
            "complaint", complaint.Id.ToString(), cancellationToken);

        if (conversationId is not null)
        {
            await _chatServiceClient.ReopenConversationAsync(
                conversationId, "Жалоба переоткрыта", cancellationToken);

            // Add manager as participant if not already
            await _chatServiceClient.AddParticipantAsync(conversationId,
                new ChatParticipant(
                    command.ManagerId.ToString(), "manager", "manager",
                    CanRead: true, CanWrite: true, CanSendInternal: true),
                cancellationToken);
        }

        return new ApproveReopenRequestResult(reopenRequest.ToDto(), complaint.ToDto());
    }
}
