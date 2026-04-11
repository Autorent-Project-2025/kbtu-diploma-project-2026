using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RespondToInfoRequest;

public sealed class RespondToInfoRequestCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public RespondToInfoRequestCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<RespondToInfoRequestResult> Handle(
        RespondToInfoRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ReporterUserId == Guid.Empty)
            throw new ValidationException("Reporter user id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        if (complaint.CreatedByUserId != command.ReporterUserId)
            throw new ValidationException("You can only respond to your own complaint.");

        complaint.RespondToInfoRequest(command.ReporterUserId, command.Message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send response notification to chat conversation
        _ = Task.Run(async () =>
        {
            try
            {
                var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
                    "complaint", complaint.Id.ToString(), cancellationToken);
                if (conversationId is not null)
                {
                    await _chatServiceClient.SendSystemMessageAsync(
                        conversationId,
                        $"Заявитель предоставил дополнительную информацию: {command.Message}",
                        internalOnly: false,
                        cancellationToken);
                }
            }
            catch { /* non-blocking */ }
        });

        return new RespondToInfoRequestResult(complaint.ToDto());
    }
}
