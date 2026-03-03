using Microsoft.Extensions.Options;
using TicketService.Application.Events;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;
using TicketService.Domain.Enums;

namespace TicketService.Infrastructure.Events;

public sealed class TicketEventPublisher : ITicketEventPublisher
{
    private readonly IIdentityProvisioningClient _identityProvisioningClient;
    private readonly IClientProvisioningClient _clientProvisioningClient;
    private readonly IPartnerProvisioningClient _partnerProvisioningClient;
    private readonly IEmailNotificationClient _emailNotificationClient;
    private readonly ActivationOptions _activationOptions;

    public TicketEventPublisher(
        IIdentityProvisioningClient identityProvisioningClient,
        IClientProvisioningClient clientProvisioningClient,
        IPartnerProvisioningClient partnerProvisioningClient,
        IEmailNotificationClient emailNotificationClient,
        IOptions<ActivationOptions> activationOptions)
    {
        _identityProvisioningClient = identityProvisioningClient;
        _clientProvisioningClient = clientProvisioningClient;
        _partnerProvisioningClient = partnerProvisioningClient;
        _emailNotificationClient = emailNotificationClient;
        _activationOptions = activationOptions.Value;
    }

    public async Task PublishApprovedAsync(TicketApprovedEvent ticketApprovedEvent, CancellationToken cancellationToken = default)
    {
        var provisionResult = await _identityProvisioningClient.ProvisionUserAsync(
            new ProvisionIdentityUserRequest(
                ticketApprovedEvent.FullName,
                ticketApprovedEvent.Email,
                ticketApprovedEvent.BirthDate ?? DateOnly.FromDateTime(ticketApprovedEvent.ReviewedAtUtc)),
            cancellationToken);

        if (ticketApprovedEvent.TicketType == TicketType.Client)
        {
            if (ticketApprovedEvent.BirthDate is null)
            {
                throw new InvalidOperationException("Birth date is required for approved client ticket.");
            }

            await _clientProvisioningClient.ProvisionClientAsync(
                new ProvisionClientProfileRequest(
                    ticketApprovedEvent.FirstName,
                    ticketApprovedEvent.LastName,
                    ticketApprovedEvent.BirthDate.Value,
                    ticketApprovedEvent.IdentityDocumentFileName,
                    ticketApprovedEvent.DriverLicenseFileName,
                    provisionResult.UserId,
                    ticketApprovedEvent.PhoneNumber,
                    ticketApprovedEvent.AvatarUrl),
                cancellationToken);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ticketApprovedEvent.IdentityDocumentFileName))
            {
                throw new InvalidOperationException("Owner identity document is required for approved partner ticket.");
            }

            var registrationDate = DateOnly.FromDateTime(ticketApprovedEvent.ReviewedAtUtc);
            var partnershipEndDate = registrationDate.AddYears(1);

            await _partnerProvisioningClient.ProvisionPartnerAsync(
                new ProvisionPartnerProfileRequest(
                    ticketApprovedEvent.FirstName,
                    ticketApprovedEvent.LastName,
                    null,
                    ticketApprovedEvent.IdentityDocumentFileName,
                    registrationDate,
                    partnershipEndDate,
                    provisionResult.UserId,
                    ticketApprovedEvent.PhoneNumber),
                cancellationToken);
        }

        var setPasswordUrl = BuildSetPasswordUrl(provisionResult.ActivationToken);

        if (ticketApprovedEvent.TicketType == TicketType.Client)
        {
            await _emailNotificationClient.SendApprovedAsync(
                new SendApprovedEmailRequest(
                    ticketApprovedEvent.Email,
                    ticketApprovedEvent.FullName,
                    provisionResult.Email,
                    setPasswordUrl),
                cancellationToken);
        }
        else
        {
            await _emailNotificationClient.SendPartnerApprovedAsync(
                new SendPartnerApprovedEmailRequest(
                    ticketApprovedEvent.Email,
                    ticketApprovedEvent.FullName,
                    provisionResult.Email,
                    setPasswordUrl),
                cancellationToken);
        }
    }

    public async Task PublishRejectedAsync(TicketRejectedEvent ticketRejectedEvent, CancellationToken cancellationToken = default)
    {
        if (ticketRejectedEvent.TicketType == TicketType.Client)
        {
            await _emailNotificationClient.SendRejectedAsync(
                new SendRejectedEmailRequest(
                    ticketRejectedEvent.Email,
                    ticketRejectedEvent.FullName,
                    ticketRejectedEvent.DecisionReason),
                cancellationToken);
        }
        else
        {
            await _emailNotificationClient.SendPartnerRejectedAsync(
                new SendPartnerRejectedEmailRequest(
                    ticketRejectedEvent.Email,
                    ticketRejectedEvent.FullName,
                    ticketRejectedEvent.DecisionReason),
                cancellationToken);
        }
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
}
