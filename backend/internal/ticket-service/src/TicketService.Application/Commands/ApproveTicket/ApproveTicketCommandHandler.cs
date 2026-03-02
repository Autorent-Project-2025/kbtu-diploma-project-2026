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

    public ApproveTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        ITicketEventPublisher ticketEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _ticketEventPublisher = ticketEventPublisher;
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

        ticket.Approve(command.ManagerId, DateTime.UtcNow);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        await _ticketEventPublisher.PublishApprovedAsync(
            new TicketApprovedEvent(
                ticket.Id,
                ticket.FullName,
                ticket.Email,
                ticket.BirthDate,
                command.ManagerId),
            cancellationToken);

        return new ApproveTicketResult(ticket.ToDto());
    }
}
