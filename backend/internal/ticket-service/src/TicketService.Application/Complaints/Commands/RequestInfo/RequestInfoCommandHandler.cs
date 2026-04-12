using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RequestInfo;

public sealed class RequestInfoCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public RequestInfoCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<RequestInfoResult> Handle(
        RequestInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        complaint.RequestInfo(command.ManagerId, command.Message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send info request as a system message to chat conversation
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
                        $"Менеджер запросил дополнительную информацию: {command.Message}",
                        internalOnly: false,
                        cancellationToken);
                }
            }
            catch { /* non-blocking */ }
        });

        return new RequestInfoResult(complaint.ToDto());
    }
}
