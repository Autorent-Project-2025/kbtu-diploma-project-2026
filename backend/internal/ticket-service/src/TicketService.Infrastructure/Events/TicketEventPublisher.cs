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
    private readonly IPartnerCarProvisioningClient _partnerCarProvisioningClient;
    private readonly IEmailNotificationClient _emailNotificationClient;
    private readonly ActivationOptions _activationOptions;

    public TicketEventPublisher(
        IIdentityProvisioningClient identityProvisioningClient,
        IClientProvisioningClient clientProvisioningClient,
        IPartnerProvisioningClient partnerProvisioningClient,
        IPartnerCarProvisioningClient partnerCarProvisioningClient,
        IEmailNotificationClient emailNotificationClient,
        IOptions<ActivationOptions> activationOptions)
    {
        _identityProvisioningClient = identityProvisioningClient;
        _clientProvisioningClient = clientProvisioningClient;
        _partnerProvisioningClient = partnerProvisioningClient;
        _partnerCarProvisioningClient = partnerCarProvisioningClient;
        _emailNotificationClient = emailNotificationClient;
        _activationOptions = activationOptions.Value;
    }

    public async Task PublishApprovedAsync(TicketApprovedEvent ticketApprovedEvent, CancellationToken cancellationToken = default)
    {
        if (ticketApprovedEvent.TicketType == TicketType.PartnerCar)
        {
            if (!ticketApprovedEvent.RelatedPartnerUserId.HasValue || ticketApprovedEvent.RelatedPartnerUserId == Guid.Empty)
            {
                throw new InvalidOperationException("Related partner user id is required for approved partner car ticket.");
            }

            var carBrand = RequireField(ticketApprovedEvent.CarBrand, nameof(ticketApprovedEvent.CarBrand));
            var carModel = RequireField(ticketApprovedEvent.CarModel, nameof(ticketApprovedEvent.CarModel));
            var carYear = RequireYear(ticketApprovedEvent.CarYear, nameof(ticketApprovedEvent.CarYear));
            var licensePlate = RequireField(ticketApprovedEvent.LicensePlate, nameof(ticketApprovedEvent.LicensePlate));
            var ownershipDocument = RequireField(ticketApprovedEvent.OwnershipDocumentFileName, nameof(ticketApprovedEvent.OwnershipDocumentFileName));

            if (ticketApprovedEvent.CarImages.Count == 0)
            {
                throw new InvalidOperationException("At least one car image is required for approved partner car ticket.");
            }

            await _partnerCarProvisioningClient.ProvisionPartnerCarAsync(
                new ProvisionPartnerCarRequest(
                    ticketApprovedEvent.RelatedPartnerUserId.Value,
                    carBrand,
                    carModel,
                    carYear,
                    licensePlate,
                    ownershipDocument,
                    ticketApprovedEvent.CarImages
                        .Select(image => new ProvisionPartnerCarImageRequest(image.ImageId, image.ImageUrl))
                        .ToArray()),
                cancellationToken);

            await _emailNotificationClient.SendPartnerCarApprovedAsync(
                new SendPartnerCarApprovedEmailRequest(
                    ticketApprovedEvent.Email,
                    ticketApprovedEvent.FullName,
                    carBrand,
                    carModel,
                    licensePlate),
                cancellationToken);

            return;
        }

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
        else if (ticketRejectedEvent.TicketType == TicketType.PartnerCar)
        {
            await _emailNotificationClient.SendPartnerCarRejectedAsync(
                new SendPartnerCarRejectedEmailRequest(
                    ticketRejectedEvent.Email,
                    ticketRejectedEvent.FullName,
                    RequireField(ticketRejectedEvent.CarBrand, nameof(ticketRejectedEvent.CarBrand)),
                    RequireField(ticketRejectedEvent.CarModel, nameof(ticketRejectedEvent.CarModel)),
                    RequireField(ticketRejectedEvent.LicensePlate, nameof(ticketRejectedEvent.LicensePlate)),
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

    private static string RequireField(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value;
    }

    private static int RequireYear(int? value, string fieldName)
    {
        if (!value.HasValue)
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        var maxAllowedCarYear = DateTime.UtcNow.Year + 1;
        if (value.Value < 1886 || value.Value > maxAllowedCarYear)
        {
            throw new InvalidOperationException($"{fieldName} must be between 1886 and {maxAllowedCarYear}.");
        }

        return value.Value;
    }
}
