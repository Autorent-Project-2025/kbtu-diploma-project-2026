using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;

namespace TicketService.Application.Complaints.Commands.CancelComplaintBooking;

public sealed class CancelComplaintBookingCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IBookingAdminClient _bookingAdminClient;
    private readonly IBookingReadClient _bookingReadClient;
    private readonly IChatServiceClient _chatServiceClient;

    public CancelComplaintBookingCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IBookingAdminClient bookingAdminClient,
        IBookingReadClient bookingReadClient,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _bookingAdminClient = bookingAdminClient;
        _bookingReadClient = bookingReadClient;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<CancelComplaintBookingResult> Handle(
        CancelComplaintBookingCommand command,
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

        if (complaint.AssignedToManagerId is null)
            throw new ValidationException("Complaint must be assigned to a manager before actions can be performed.");

        // Verify booking is cancelable by reading current status
        var booking = await _bookingReadClient.GetBookingAsync(complaint.BookingId, cancellationToken);
        if (booking is null)
            throw new NotFoundException($"Booking '{complaint.BookingId}' was not found.");

        var bookingStatus = booking.Status.Trim().ToLowerInvariant();
        if (bookingStatus is "completed" or "canceled")
            throw new ValidationException($"Booking cannot be canceled because its status is '{booking.Status}'.");
        if (bookingStatus is "active" or "awaitingreview")
            throw new ValidationException($"Booking in status '{booking.Status}' cannot be canceled through a complaint. Active or post-rental bookings require a separate process.");

        // Call booking-service to cancel via its domain logic
        var canceled = await _bookingAdminClient.CancelBookingAsync(complaint.BookingId, cancellationToken);
        if (!canceled)
            throw new InvalidOperationException($"Failed to cancel booking {complaint.BookingId}.");

        // Record action log
        var actionLog = ComplaintActionLog.Create(
            complaint.Id,
            "CancelBooking",
            command.ManagerId,
            command.Reason,
            "Booking",
            complaint.BookingId.ToString());

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
                    $"Менеджер отменил связанное бронирование #{complaint.BookingId}. Причина: {command.Reason.Trim()}",
                    ct: cancellationToken);
            }
        }
        catch
        {
            // Chat notification is non-critical; action already succeeded
        }

        return new CancelComplaintBookingResult(complaint.ToDto());
    }
}

public sealed record CancelComplaintBookingResult(ComplaintDto Complaint);
