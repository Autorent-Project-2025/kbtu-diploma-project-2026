using TicketService.Application.Events;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Commands.IssueTicketFine;

public sealed class IssueTicketFineCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketUnitOfWork _ticketUnitOfWork;
    private readonly ITicketEventPublisher _ticketEventPublisher;

    public IssueTicketFineCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        ITicketEventPublisher ticketEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _ticketEventPublisher = ticketEventPublisher;
    }

    public async Task<IssueTicketFineResult> Handle(
        IssueTicketFineCommand command,
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

        var reviewedAtUtc = DateTime.UtcNow;
        ticket.IssueFine(command.ManagerId, command.Amount, reviewedAtUtc);
        await _ticketEventPublisher.PublishFineIssuedAsync(
            new TicketFineIssuedEvent(
                ticket.Id,
                ticket.TicketType,
                command.Amount,
                command.ManagerId,
                reviewedAtUtc),
            cancellationToken);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        return new IssueTicketFineResult(ticket.ToDto());
    }
}
