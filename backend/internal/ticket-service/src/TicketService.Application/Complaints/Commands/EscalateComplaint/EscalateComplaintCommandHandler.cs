using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;

namespace TicketService.Application.Complaints.Commands.EscalateComplaint;

public sealed class EscalateComplaintCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public EscalateComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<EscalateComplaintResult> Handle(
        EscalateComplaintCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");
        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ValidationException("Reason is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        // Domain method validates: not terminal, not already escalated
        complaint.Escalate(command.ManagerId, command.Reason);

        // Record action log
        var actionLog = ComplaintActionLog.Create(
            complaint.Id,
            "Escalate",
            command.ManagerId,
            command.Reason);

        await _complaintRepository.AddActionLogAsync(actionLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send system message to complaint chat
        try
        {
            var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
                "complaint", command.ComplaintId.ToString(), cancellationToken);

            if (conversationId is not null)
            {
                await _chatServiceClient.SendSystemMessageAsync(
                    conversationId,
                    $"Жалоба эскалирована суперменеджеру. Причина: {command.Reason.Trim()}",
                    internalOnly: true,
                    ct: cancellationToken);
            }
        }
        catch
        {
            // Chat notification is non-critical
        }

        return new EscalateComplaintResult(complaint.ToDto());
    }
}

public sealed record EscalateComplaintResult(ComplaintDto Complaint);
