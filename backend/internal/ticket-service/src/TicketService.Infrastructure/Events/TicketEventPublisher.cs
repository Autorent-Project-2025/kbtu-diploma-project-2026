using Microsoft.Extensions.Options;
using TicketService.Application.Events;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Events;

public sealed class TicketEventPublisher : ITicketEventPublisher
{
    private readonly IIdentityProvisioningClient _identityProvisioningClient;
    private readonly IClientProvisioningClient _clientProvisioningClient;
    private readonly IEmailNotificationClient _emailNotificationClient;
    private readonly ActivationOptions _activationOptions;

    public TicketEventPublisher(
        IIdentityProvisioningClient identityProvisioningClient,
        IClientProvisioningClient clientProvisioningClient,
        IEmailNotificationClient emailNotificationClient,
        IOptions<ActivationOptions> activationOptions)
    {
        _identityProvisioningClient = identityProvisioningClient;
        _clientProvisioningClient = clientProvisioningClient;
        _emailNotificationClient = emailNotificationClient;
        _activationOptions = activationOptions.Value;
    }

    public async Task PublishApprovedAsync(TicketApprovedEvent ticketApprovedEvent, CancellationToken cancellationToken = default)
    {
        var provisionResult = await _identityProvisioningClient.ProvisionUserAsync(
            new ProvisionIdentityUserRequest(
                ticketApprovedEvent.FullName,
                ticketApprovedEvent.Email,
                ticketApprovedEvent.BirthDate),
            cancellationToken);

        var (firstName, lastName) = SplitFullName(ticketApprovedEvent.FullName);
        await _clientProvisioningClient.ProvisionClientAsync(
            new ProvisionClientProfileRequest(
                firstName,
                lastName,
                ticketApprovedEvent.BirthDate,
                ticketApprovedEvent.IdentityDocumentFileName,
                ticketApprovedEvent.DriverLicenseFileName,
                provisionResult.UserId,
                ticketApprovedEvent.PhoneNumber,
                ticketApprovedEvent.AvatarUrl),
            cancellationToken);

        var setPasswordUrl = BuildSetPasswordUrl(provisionResult.ActivationToken);
        await _emailNotificationClient.SendApprovedAsync(
            new SendApprovedEmailRequest(
                ticketApprovedEvent.Email,
                ticketApprovedEvent.FullName,
                provisionResult.Email,
                setPasswordUrl),
            cancellationToken);
    }

    public async Task PublishRejectedAsync(TicketRejectedEvent ticketRejectedEvent, CancellationToken cancellationToken = default)
    {
        await _emailNotificationClient.SendRejectedAsync(
            new SendRejectedEmailRequest(
                ticketRejectedEvent.Email,
                ticketRejectedEvent.FullName,
                ticketRejectedEvent.DecisionReason),
            cancellationToken);
    }

    private string BuildSetPasswordUrl(string activationToken)
    {
        if (string.IsNullOrWhiteSpace(_activationOptions.SetPasswordBaseUrl))
        {
            throw new InvalidOperationException("Activation:SetPasswordBaseUrl configuration is required.");
        }

        var baseUrl = _activationOptions.SetPasswordBaseUrl.Trim();
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{baseUrl}{separator}token={Uri.EscapeDataString(activationToken)}";
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ("Unknown", "Unknown");
        }

        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return ("Unknown", "Unknown");
        }

        if (parts.Length == 1)
        {
            return (parts[0], parts[0]);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
