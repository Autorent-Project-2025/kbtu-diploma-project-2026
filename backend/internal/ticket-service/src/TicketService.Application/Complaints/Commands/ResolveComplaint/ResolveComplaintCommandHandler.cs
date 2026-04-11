using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.ResolveComplaint;

public sealed class ResolveComplaintCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public ResolveComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<ResolveComplaintResult> Handle(
        ResolveComplaintCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        complaint.Resolve(command.ManagerId, command.ResolutionType, command.ResolutionNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
            "complaint", command.ComplaintId.ToString(), cancellationToken);

        if (conversationId is not null)
        {
            await _chatServiceClient.CloseConversationAsync(
                conversationId, "Жалоба решена", cancellationToken);
        }

        return new ResolveComplaintResult(complaint.ToDto());
    }
}
