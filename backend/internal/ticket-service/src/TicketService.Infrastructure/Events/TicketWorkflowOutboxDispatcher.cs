using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;
using TicketService.Infrastructure.Events.Outbox;
using TicketService.Infrastructure.Options;
using TicketService.Infrastructure.Persistence;

namespace TicketService.Infrastructure.Events;

public sealed class TicketWorkflowOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketWorkflowOutboxDispatcher> _logger;
    private readonly TicketWorkflowOutboxOptions _options;
    private readonly ActivationOptions _activationOptions;

    public TicketWorkflowOutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IOptions<TicketWorkflowOutboxOptions> options,
        IOptions<ActivationOptions> activationOptions,
        ILogger<TicketWorkflowOutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _activationOptions = activationOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DispatchPendingMessagesAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();

            var batch = await ClaimBatchAsync(db, cancellationToken);
            if (batch.Count == 0)
            {
                return;
            }

            foreach (var message in batch)
            {
                await DispatchMessageAsync(scope.ServiceProvider, db, message, cancellationToken);
            }
        }
    }

    private async Task<List<TicketWorkflowOutboxMessage>> ClaimBatchAsync(
        TicketDbContext db,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lockUntil = now.AddSeconds(_options.LockTimeoutSeconds);

        var messages = await db.TicketWorkflowOutboxMessages
            .Where(message =>
                message.ProcessedAt == null &&
                message.NextAttemptAt <= now &&
                (message.LockedUntil == null || message.LockedUntil <= now))
            .OrderBy(message => message.Id)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return messages;
        }

        foreach (var message in messages)
        {
            message.LockedUntil = lockUntil;
        }

        await db.SaveChangesAsync(cancellationToken);
        return messages;
    }

    private async Task DispatchMessageAsync(
        IServiceProvider serviceProvider,
        TicketDbContext db,
        TicketWorkflowOutboxMessage message,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        try
        {
            switch (message.EventType)
            {
                case TicketWorkflowOutboxEventTypes.Approved:
                {
                    var payload = TicketWorkflowPayloadSerializer.Deserialize<TicketApprovedWorkflowPayload>(message.Payload);
                    await ProcessApprovedWorkflowAsync(serviceProvider, db, message, payload, cancellationToken);
                    break;
                }

                case TicketWorkflowOutboxEventTypes.Rejected:
                {
                    var payload = TicketWorkflowPayloadSerializer.Deserialize<TicketRejectedWorkflowPayload>(message.Payload);
                    await ProcessRejectedWorkflowAsync(serviceProvider, db, message, payload, cancellationToken);
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unsupported ticket workflow outbox event type '{message.EventType}'.");
            }

            message.AttemptCount += 1;
            message.ProcessedAt = now;
            message.LockedUntil = null;
            message.LastError = null;

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            message.AttemptCount += 1;
            message.LockedUntil = null;
            message.LastError = TruncateError(ex.Message);
            message.NextAttemptAt = now.Add(ComputeRetryDelay(message.AttemptCount));

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                ex,
                "Ticket workflow dispatch failed for message {OutboxMessageId} ({EventType}). Attempt {AttemptCount}.",
                message.Id,
                message.EventType,
                message.AttemptCount);
        }
    }

    private async Task ProcessApprovedWorkflowAsync(
        IServiceProvider serviceProvider,
        TicketDbContext db,
        TicketWorkflowOutboxMessage message,
        TicketApprovedWorkflowPayload payload,
        CancellationToken cancellationToken)
    {
        var ticket = await LoadRequiredTicketAsync(db, payload.TicketId, cancellationToken);
        var identityProvisioningClient = serviceProvider.GetRequiredService<IIdentityProvisioningClient>();
        var clientProvisioningClient = serviceProvider.GetRequiredService<IClientProvisioningClient>();
        var partnerProvisioningClient = serviceProvider.GetRequiredService<IPartnerProvisioningClient>();
        var rabbitMqPublisher = serviceProvider.GetRequiredService<IRabbitMqPublisher>();

        while (payload.CurrentStep != TicketApprovedWorkflowStep.Completed)
        {
            switch (payload.CurrentStep)
            {
                case TicketApprovedWorkflowStep.ProvisionIdentity:
                {
                    var provisionResult = await identityProvisioningClient.ProvisionUserAsync(
                        new ProvisionIdentityUserRequest(
                            ticket.FullName,
                            ticket.Email,
                            ticket.BirthDate ?? DateOnly.FromDateTime(ticket.ReviewedAt ?? DateTime.UtcNow),
                            BuildIdentityRequestKey(ticket.Id),
                            SubjectType: "user",
                            ActorType: ticket.TicketType == TicketType.Partner ? "partner" : "client"),
                        cancellationToken);

                    payload.ProvisionedUserId = provisionResult.UserId;
                    payload.LoginEmail = provisionResult.Email;
                    payload.ActivationToken = provisionResult.ActivationToken;
                    payload.CurrentStep = TicketApprovedWorkflowStep.ProvisionProfile;
                    break;
                }

                case TicketApprovedWorkflowStep.ProvisionProfile:
                {
                    var provisionedUserId = RequireGuid(payload.ProvisionedUserId, nameof(payload.ProvisionedUserId));
                    if (ticket.TicketType == TicketType.Client)
                    {
                        await clientProvisioningClient.ProvisionClientAsync(
                            new ProvisionClientProfileRequest(
                                ticket.FirstName,
                                ticket.LastName,
                                ticket.BirthDate ?? throw new InvalidOperationException("Birth date is required for approved client ticket."),
                                ticket.IdentityDocumentFileName,
                                ticket.DriverLicenseFileName,
                                provisionedUserId,
                                ticket.PhoneNumber,
                                ticket.AvatarUrl,
                                BuildClientProfileRequestKey(ticket.Id)),
                            cancellationToken);
                    }
                    else if (ticket.TicketType == TicketType.Partner)
                    {
                        var reviewedAt = ticket.ReviewedAt ?? DateTime.UtcNow;
                        var registrationDate = DateOnly.FromDateTime(reviewedAt);
                        var partnershipEndDate = registrationDate.AddYears(1);

                        await partnerProvisioningClient.ProvisionPartnerAsync(
                            new ProvisionPartnerProfileRequest(
                                ticket.FirstName,
                                ticket.LastName,
                                null,
                                RequireField(ticket.IdentityDocumentFileName, nameof(ticket.IdentityDocumentFileName)),
                                registrationDate,
                                partnershipEndDate,
                                provisionedUserId,
                                ticket.PhoneNumber,
                                BuildPartnerProfileRequestKey(ticket.Id)),
                            cancellationToken);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Approved workflow step '{payload.CurrentStep}' is invalid for ticket type '{ticket.TicketType}'.");
                    }

                    payload.CurrentStep = TicketApprovedWorkflowStep.PublishApprovedEmail;
                    break;
                }

                case TicketApprovedWorkflowStep.PublishApprovedEmail:
                {
                    var loginEmail = RequireField(payload.LoginEmail, nameof(payload.LoginEmail));
                    var activationToken = RequireField(payload.ActivationToken, nameof(payload.ActivationToken));
                    var setPasswordUrl = BuildSetPasswordUrl(activationToken);

                    if (ticket.TicketType == TicketType.Client)
                    {
                        await rabbitMqPublisher.PublishAsync(
                            BuildApprovedEmailEventId(ticket.Id, ticket.TicketType),
                            RabbitMqTopology.RoutingKeys.TicketClientApprovedEmailRequested,
                            new ClientApprovedEmailRequested(
                                ticket.Id,
                                ticket.Email,
                                ticket.FullName,
                                loginEmail,
                                setPasswordUrl),
                            cancellationToken);
                    }
                    else if (ticket.TicketType == TicketType.Partner)
                    {
                        await rabbitMqPublisher.PublishAsync(
                            BuildApprovedEmailEventId(ticket.Id, ticket.TicketType),
                            RabbitMqTopology.RoutingKeys.TicketPartnerApprovedEmailRequested,
                            new PartnerApprovedEmailRequested(
                                ticket.Id,
                                ticket.Email,
                                ticket.FullName,
                                loginEmail,
                                setPasswordUrl),
                            cancellationToken);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Approved workflow step '{payload.CurrentStep}' is invalid for ticket type '{ticket.TicketType}'.");
                    }

                    payload.CurrentStep = TicketApprovedWorkflowStep.Completed;
                    break;
                }

                case TicketApprovedWorkflowStep.PublishPartnerCarProvision:
                {
                    if (ticket.TicketType != TicketType.PartnerCar)
                    {
                        throw new InvalidOperationException($"Approved workflow step '{payload.CurrentStep}' is invalid for ticket type '{ticket.TicketType}'.");
                    }

                    await rabbitMqPublisher.PublishAsync(
                        BuildPartnerCarProvisionEventId(ticket.Id),
                        RabbitMqTopology.RoutingKeys.TicketPartnerCarProvisionRequested,
                        new PartnerCarProvisionRequested(
                            ticket.Id,
                            BuildPartnerCarProvisionRequestKey(ticket.Id),
                            RequireGuid(ticket.RelatedPartnerUserId, nameof(ticket.RelatedPartnerUserId)),
                            RequireField(ticket.CarBrand, nameof(ticket.CarBrand)),
                            RequireField(ticket.CarModel, nameof(ticket.CarModel)),
                            RequireYear(ticket.CarYear, nameof(ticket.CarYear)),
                            RequireField(ticket.LicensePlate, nameof(ticket.LicensePlate)),
                            RequirePrice(ticket.PriceHour, nameof(ticket.PriceHour)),
                            RequirePrice(ticket.PriceDay, nameof(ticket.PriceDay)),
                            RequireField(ticket.OwnershipDocumentFileName, nameof(ticket.OwnershipDocumentFileName)),
                            ticket.CarImages.Select(image => new PartnerCarProvisionRequestedImage(image.ImageId, image.ImageUrl)).ToArray()),
                        cancellationToken);

                    payload.CurrentStep = TicketApprovedWorkflowStep.PublishPartnerCarApprovedEmail;
                    break;
                }

                case TicketApprovedWorkflowStep.PublishPartnerCarApprovedEmail:
                {
                    if (ticket.TicketType != TicketType.PartnerCar)
                    {
                        throw new InvalidOperationException($"Approved workflow step '{payload.CurrentStep}' is invalid for ticket type '{ticket.TicketType}'.");
                    }

                    await rabbitMqPublisher.PublishAsync(
                        BuildApprovedEmailEventId(ticket.Id, ticket.TicketType),
                        RabbitMqTopology.RoutingKeys.TicketPartnerCarApprovedEmailRequested,
                        new PartnerCarApprovedEmailRequested(
                            ticket.Id,
                            ticket.Email,
                            ticket.FullName,
                            RequireField(ticket.CarBrand, nameof(ticket.CarBrand)),
                            RequireField(ticket.CarModel, nameof(ticket.CarModel)),
                            RequireField(ticket.LicensePlate, nameof(ticket.LicensePlate))),
                        cancellationToken);

                    payload.CurrentStep = TicketApprovedWorkflowStep.Completed;
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unsupported approved ticket workflow step '{payload.CurrentStep}'.");
            }

            message.Payload = TicketWorkflowPayloadSerializer.Serialize(payload);
        }
    }

    private async Task ProcessRejectedWorkflowAsync(
        IServiceProvider serviceProvider,
        TicketDbContext db,
        TicketWorkflowOutboxMessage message,
        TicketRejectedWorkflowPayload payload,
        CancellationToken cancellationToken)
    {
        var ticket = await LoadRequiredTicketAsync(db, payload.TicketId, cancellationToken);
        var rabbitMqPublisher = serviceProvider.GetRequiredService<IRabbitMqPublisher>();

        while (payload.CurrentStep != TicketRejectedWorkflowStep.Completed)
        {
            switch (payload.CurrentStep)
            {
                case TicketRejectedWorkflowStep.PublishRejectedEmail:
                {
                    if (ticket.TicketType == TicketType.Client)
                    {
                        await rabbitMqPublisher.PublishAsync(
                            BuildRejectedEmailEventId(ticket.Id, ticket.TicketType),
                            RabbitMqTopology.RoutingKeys.TicketClientRejectedEmailRequested,
                            new ClientRejectedEmailRequested(
                                ticket.Id,
                                ticket.Email,
                                ticket.FullName,
                                ticket.DecisionReason),
                            cancellationToken);
                    }
                    else if (ticket.TicketType == TicketType.PartnerCar)
                    {
                        await rabbitMqPublisher.PublishAsync(
                            BuildRejectedEmailEventId(ticket.Id, ticket.TicketType),
                            RabbitMqTopology.RoutingKeys.TicketPartnerCarRejectedEmailRequested,
                            new PartnerCarRejectedEmailRequested(
                                ticket.Id,
                                ticket.Email,
                                ticket.FullName,
                                RequireField(ticket.CarBrand, nameof(ticket.CarBrand)),
                                RequireField(ticket.CarModel, nameof(ticket.CarModel)),
                                RequireField(ticket.LicensePlate, nameof(ticket.LicensePlate)),
                                RequireField(ticket.DecisionReason, nameof(ticket.DecisionReason))),
                            cancellationToken);
                    }
                    else
                    {
                        await rabbitMqPublisher.PublishAsync(
                            BuildRejectedEmailEventId(ticket.Id, ticket.TicketType),
                            RabbitMqTopology.RoutingKeys.TicketPartnerRejectedEmailRequested,
                            new PartnerRejectedEmailRequested(
                                ticket.Id,
                                ticket.Email,
                                ticket.FullName,
                                ticket.DecisionReason),
                            cancellationToken);
                    }

                    payload.CurrentStep = TicketRejectedWorkflowStep.Completed;
                    message.Payload = TicketWorkflowPayloadSerializer.Serialize(payload);
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unsupported rejected ticket workflow step '{payload.CurrentStep}'.");
            }
        }
    }

    private static async Task<Ticket> LoadRequiredTicketAsync(
        TicketDbContext db,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == ticketId, cancellationToken);

        return ticket ?? throw new KeyNotFoundException($"Ticket '{ticketId}' was not found for outbox dispatch.");
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

    private static string BuildIdentityRequestKey(Guid ticketId) => $"ticket:{ticketId}:identity";

    private static string BuildClientProfileRequestKey(Guid ticketId) => $"ticket:{ticketId}:client-profile";

    private static string BuildPartnerProfileRequestKey(Guid ticketId) => $"ticket:{ticketId}:partner-profile";

    private static string BuildPartnerCarProvisionRequestKey(Guid ticketId) => $"ticket:{ticketId}:partner-car";

    private static string BuildApprovedEmailEventId(Guid ticketId, TicketType ticketType) => ticketType switch
    {
        TicketType.Client => $"ticket:{ticketId}:client-approved-email",
        TicketType.Partner => $"ticket:{ticketId}:partner-approved-email",
        TicketType.PartnerCar => $"ticket:{ticketId}:partner-car-approved-email",
        _ => throw new InvalidOperationException($"Unsupported ticket type '{ticketType}'.")
    };

    private static string BuildRejectedEmailEventId(Guid ticketId, TicketType ticketType) => ticketType switch
    {
        TicketType.Client => $"ticket:{ticketId}:client-rejected-email",
        TicketType.Partner => $"ticket:{ticketId}:partner-rejected-email",
        TicketType.PartnerCar => $"ticket:{ticketId}:partner-car-rejected-email",
        _ => throw new InvalidOperationException($"Unsupported ticket type '{ticketType}'.")
    };

    private static string BuildPartnerCarProvisionEventId(Guid ticketId) => $"ticket:{ticketId}:partner-car-provision";

    private TimeSpan ComputeRetryDelay(int attemptCount)
    {
        var factor = Math.Pow(2, Math.Max(0, attemptCount - 1));
        var seconds = _options.InitialRetryDelaySeconds * factor;
        var cappedSeconds = Math.Min(seconds, _options.MaxRetryDelaySeconds);

        return TimeSpan.FromSeconds(cappedSeconds);
    }

    private static string TruncateError(string error)
    {
        const int maxLength = 4000;
        return error.Length <= maxLength ? error : error[..maxLength];
    }

    private static string RequireField(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value;
    }

    private static Guid RequireGuid(Guid? value, string fieldName)
    {
        if (!value.HasValue || value.Value == Guid.Empty)
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Value;
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

    private static decimal RequirePrice(decimal? value, string fieldName)
    {
        if (!value.HasValue)
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        if (value.Value <= 0m)
        {
            throw new InvalidOperationException($"{fieldName} must be greater than 0.");
        }

        if (value.Value > 1_000_000m)
        {
            throw new InvalidOperationException($"{fieldName} must not exceed 1000000.");
        }

        return decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero);
    }
}
