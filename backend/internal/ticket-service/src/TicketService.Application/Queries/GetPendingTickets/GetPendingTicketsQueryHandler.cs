using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetPendingTickets;

public sealed class GetPendingTicketsQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetPendingTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetPendingTicketsResult> Handle(
        GetPendingTicketsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetPendingAsync(cancellationToken);
        var ticketDtos = tickets
            .Select(ticket => ticket.ToDto())
            .ToArray();

        return new GetPendingTicketsResult(ticketDtos);
    }
}
