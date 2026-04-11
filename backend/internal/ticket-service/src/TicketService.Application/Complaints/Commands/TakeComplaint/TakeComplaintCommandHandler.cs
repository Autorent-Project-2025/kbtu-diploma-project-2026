using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.TakeComplaint;

public sealed class TakeComplaintCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public TakeComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<TakeComplaintResult> Handle(
        TakeComplaintCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        complaint.Take(command.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
            "complaint", command.ComplaintId.ToString(), cancellationToken);

        if (conversationId is not null)
        {
            await _chatServiceClient.AddParticipantAsync(conversationId,
                new ChatParticipant(
                    command.ManagerId.ToString(), "manager", "manager",
                    CanRead: true, CanWrite: true, CanSendInternal: true),
                cancellationToken);

            await _chatServiceClient.SendSystemMessageAsync(
                conversationId, "Менеджер взял жалобу в работу", ct: cancellationToken);
        }

        return new TakeComplaintResult(complaint.ToDto());
    }
}
