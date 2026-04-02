namespace BookingService.Application.Interfaces.Integrations;

public interface IBookingEmailClient
{
    Task SendCustomEmailAsync(
        string to,
        string subject,
        string text,
        string? html = null,
        CancellationToken cancellationToken = default);
}
