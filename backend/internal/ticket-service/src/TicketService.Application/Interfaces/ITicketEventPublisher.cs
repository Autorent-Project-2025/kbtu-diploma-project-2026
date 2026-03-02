using TicketService.Application.Events;

namespace TicketService.Application.Interfaces;

public interface ITicketEventPublisher
{
    Task PublishApprovedAsync(TicketApprovedEvent ticketApprovedEvent, CancellationToken cancellationToken = default);

    Task PublishRejectedAsync(TicketRejectedEvent ticketRejectedEvent, CancellationToken cancellationToken = default);
}
