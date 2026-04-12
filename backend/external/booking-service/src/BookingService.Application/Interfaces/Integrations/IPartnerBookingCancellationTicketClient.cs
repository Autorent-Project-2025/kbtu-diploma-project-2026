namespace BookingService.Application.Interfaces.Integrations;

public interface IPartnerBookingCancellationTicketClient
{
    Task<PartnerBookingCancellationTicketPayload> CreatePartnerBookingCancellationTicketAsync(
        PartnerBookingCancellationTicketCreatePayload payload,
        CancellationToken cancellationToken = default);
}
