using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetTicketById;

public sealed class GetTicketByIdQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetTicketByIdResult> Handle(
        GetTicketByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.TicketId == Guid.Empty)
        {
            throw new ValidationException("Ticket id is required.");
        }

        var ticket = await _ticketRepository.GetByIdAsync(query.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new NotFoundException($"Ticket '{query.TicketId}' was not found.");
        }

        return new GetTicketByIdResult(ticket.ToDto());
    }
}
