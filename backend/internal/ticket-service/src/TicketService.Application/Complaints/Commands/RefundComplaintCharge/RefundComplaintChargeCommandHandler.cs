using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;

namespace TicketService.Application.Complaints.Commands.RefundComplaintCharge;

public sealed class RefundComplaintChargeCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IPaymentClient _paymentClient;
    private readonly IChatServiceClient _chatServiceClient;

    public RefundComplaintChargeCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IPaymentClient paymentClient,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _paymentClient = paymentClient;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<RefundComplaintChargeResult> Handle(
        RefundComplaintChargeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");
        if (command.ChargeId <= 0)
            throw new ValidationException("Charge id must be greater than zero.");
        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ValidationException("Reason is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        if (complaint.AssignedToManagerId is null)
            throw new ValidationException("Complaint must be assigned to a manager before actions can be performed.");

        // Verify the charge belongs to this complaint's booking
        var charges = await _paymentClient.GetBookingChargesAsync(complaint.BookingId, cancellationToken);
        var targetCharge = charges.FirstOrDefault(c => c.Id == command.ChargeId);

        if (targetCharge is null)
            throw new ValidationException($"Charge {command.ChargeId} is not associated with booking {complaint.BookingId}.");

        if (!targetCharge.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException($"Only paid charges can be refunded. Current status: {targetCharge.Status}.");

        // Refund the paid charge via payment service
        var refunded = await _paymentClient.RefundBookingChargeAsync(command.ChargeId, command.Reason, cancellationToken);
        if (!refunded)
            throw new InvalidOperationException($"Failed to refund charge {command.ChargeId}.");

        // Record action log
        var actionLog = ComplaintActionLog.Create(
            complaint.Id,
            "RefundCharge",
            command.ManagerId,
            command.Reason,
            "BookingCharge",
            command.ChargeId.ToString());

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
                    $"Менеджер вернул средства по начислению #{command.ChargeId} ({targetCharge.ChargeType}, {targetCharge.Amount:N0} KZT). Причина: {command.Reason.Trim()}",
                    ct: cancellationToken);
            }
        }
        catch
        {
            // Chat notification is non-critical
        }

        return new RefundComplaintChargeResult(complaint.ToDto());
    }
}

public sealed record RefundComplaintChargeResult(ComplaintDto Complaint);
