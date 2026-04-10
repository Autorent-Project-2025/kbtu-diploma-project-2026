using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetAllTickets;

public sealed class GetAllTicketsQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetAllTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetAllTicketsResult> Handle(
        GetAllTicketsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAllAsync(cancellationToken);
        var ticketDtos = tickets
            .Select(ticket => ticket.ToDto())
            .ToArray();

        return new GetAllTicketsResult(ticketDtos);
    }
}
