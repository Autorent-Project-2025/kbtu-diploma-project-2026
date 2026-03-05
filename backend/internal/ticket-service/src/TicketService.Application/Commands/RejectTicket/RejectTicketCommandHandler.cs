using TicketService.Application.Events;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Commands.RejectTicket;

public sealed class RejectTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketUnitOfWork _ticketUnitOfWork;
    private readonly ITicketEventPublisher _ticketEventPublisher;

    public RejectTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        ITicketEventPublisher ticketEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _ticketEventPublisher = ticketEventPublisher;
    }

    public async Task<RejectTicketResult> Handle(
        RejectTicketCommand command,
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

        if (string.IsNullOrWhiteSpace(command.DecisionReason))
        {
            throw new ValidationException("Decision reason is required.");
        }

        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new NotFoundException($"Ticket '{command.TicketId}' was not found.");
        }

        ApplyPartnerCarReviewDataIfNeeded(ticket, command.PartnerCarData);

        ticket.Reject(command.ManagerId, command.DecisionReason, DateTime.UtcNow);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        await _ticketEventPublisher.PublishRejectedAsync(
            new TicketRejectedEvent(
                ticket.Id,
                ticket.TicketType,
                ticket.FullName,
                ticket.Email,
                ticket.CarBrand,
                ticket.CarModel,
                ticket.LicensePlate,
                command.DecisionReason.Trim(),
                command.ManagerId),
            cancellationToken);

        return new RejectTicketResult(ticket.ToDto());
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
            partnerCarData.Email);
    }
}
