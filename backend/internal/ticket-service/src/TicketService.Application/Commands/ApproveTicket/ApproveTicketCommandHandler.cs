using TicketService.Application.Events;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Commands.ApproveTicket;

public sealed class ApproveTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketUnitOfWork _ticketUnitOfWork;
    private readonly ITicketEventPublisher _ticketEventPublisher;
    private readonly IBookingReadClient _bookingReadClient;

    public ApproveTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        ITicketEventPublisher ticketEventPublisher,
        IBookingReadClient bookingReadClient)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _ticketEventPublisher = ticketEventPublisher;
        _bookingReadClient = bookingReadClient;
    }

    public async Task<ApproveTicketResult> Handle(
        ApproveTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TicketId == Guid.Empty)
        {
            throw new ValidationException("Ticket id is required.");
        }

        if (command.ManagerId == Guid.Empty)
        {
            throw new ValidationException("Manager id is required.");
        }

        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new NotFoundException($"Ticket '{command.TicketId}' was not found.");
        }

        ApplyPartnerCarReviewDataIfNeeded(ticket, command.PartnerCarData);
        await ValidatePartnerBookingCancellationAsync(ticket, cancellationToken);

        var reviewedAtUtc = DateTime.UtcNow;
        ticket.Approve(command.ManagerId, reviewedAtUtc);

        await _ticketEventPublisher.PublishApprovedAsync(
            new TicketApprovedEvent(
                ticket.Id,
                ticket.TicketType,
                ticket.FirstName,
                ticket.LastName,
                ticket.FullName,
                ticket.Email,
                ticket.BirthDate,
                ticket.PhoneNumber,
                ticket.IdentityDocumentFileName,
                ticket.DriverLicenseFileName,
                ticket.AvatarUrl,
                ticket.RelatedPartnerUserId,
                ticket.CarBrand,
                ticket.CarModel,
                ticket.CarYear,
                ticket.LicensePlate,
                ticket.OwnershipDocumentFileName,
                ticket.CarImages,
                command.ManagerId,
                reviewedAtUtc),
            cancellationToken);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveTicketResult(ticket.ToDto());
    }

    private static void ApplyPartnerCarReviewDataIfNeeded(
        Domain.Entities.Ticket ticket,
        PartnerCarTicketReviewData? partnerCarData)
    {
        if (partnerCarData is null)
        {
            return;
        }

        ticket.UpdatePartnerCarDetailsForReview(
            partnerCarData.CarBrand,
            partnerCarData.CarModel,
            partnerCarData.CarYear,
            partnerCarData.LicensePlate,
            partnerCarData.Color,
            partnerCarData.RequestedStatus,
            partnerCarData.IsActive,
            partnerCarData.Transmission,
            partnerCarData.FuelType,
            partnerCarData.Seats,
            partnerCarData.Doors,
            partnerCarData.BodyType,
            partnerCarData.Horsepower,
            partnerCarData.ConfirmedTags);
    }

    private async Task ValidatePartnerBookingCancellationAsync(
        Domain.Entities.Ticket ticket,
        CancellationToken cancellationToken)
    {
        if (ticket.TicketType != Domain.Enums.TicketType.PartnerBookingCancellation)
        {
            return;
        }

        var bookingId = ticket.BookingId ?? 0;
        if (bookingId <= 0)
        {
            throw new ValidationException("Booking id is required for partner cancellation tickets.");
        }

        var booking = await _bookingReadClient.GetBookingAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            throw new NotFoundException($"Booking '{bookingId}' was not found.");
        }

        var bookingStatus = booking.Status.Trim().ToLowerInvariant();
        if (bookingStatus is "completed" or "canceled")
        {
            throw new ValidationException($"Booking cannot be canceled because its status is '{booking.Status}'.");
        }
    }
}
