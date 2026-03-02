using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IEmailNotificationClient
{
    Task SendApprovedAsync(
        SendApprovedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendRejectedAsync(
        SendRejectedEmailRequest request,
        CancellationToken cancellationToken = default);
}
