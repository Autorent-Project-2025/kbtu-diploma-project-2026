namespace BookingService.Application.Interfaces.Integrations;

public interface IBookingCompletionTicketClient
{
    Task<BookingCompletionTicketPayload> CreateBookingCompletionTicketAsync(
        BookingCompletionTicketCreatePayload payload,
        CancellationToken cancellationToken = default);
}
