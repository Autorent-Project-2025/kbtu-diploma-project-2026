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

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var q = query.Search.Trim().ToLower();
            var isGuid = Guid.TryParse(query.Search.Trim(), out var searchGuid);

            ticketDtos = ticketDtos.Where(t =>
                (isGuid && t.Id == searchGuid) ||
                (t.FullName != null && t.FullName.ToLower().Contains(q)) ||
                (t.Email != null && t.Email.ToLower().Contains(q)) ||
                (t.PhoneNumber != null && t.PhoneNumber.ToLower().Contains(q))
            ).ToArray();
        }

        return new GetAllTicketsResult(ticketDtos);
    }
}
