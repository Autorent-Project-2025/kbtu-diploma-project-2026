using TicketService.Application.Events;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;
using TicketService.Infrastructure.Events.Outbox;
using TicketService.Infrastructure.Persistence;

namespace TicketService.Infrastructure.Events;

public sealed class TicketEventPublisher : ITicketEventPublisher
{
    private readonly TicketDbContext _ticketDbContext;

    public TicketEventPublisher(TicketDbContext ticketDbContext)
    {
        _ticketDbContext = ticketDbContext;
    }

    public Task PublishApprovedAsync(TicketApprovedEvent ticketApprovedEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ticketApprovedEvent);

        var eventKey = $"ticket:{ticketApprovedEvent.TicketId}:approved";
        if (_ticketDbContext.TicketWorkflowOutboxMessages.Local.Any(message => message.EventKey == eventKey))
        {
            return Task.CompletedTask;
        }

        _ticketDbContext.TicketWorkflowOutboxMessages.Add(new TicketWorkflowOutboxMessage
        {
            TicketId = ticketApprovedEvent.TicketId,
            EventKey = eventKey,
            EventType = TicketWorkflowOutboxEventTypes.Approved,
            Payload = TicketWorkflowPayloadSerializer.Serialize(new TicketApprovedWorkflowPayload
            {
                TicketId = ticketApprovedEvent.TicketId,
                CurrentStep = ticketApprovedEvent.TicketType == TicketType.PartnerCar
                    ? TicketApprovedWorkflowStep.PublishPartnerCarProvision
                    : TicketApprovedWorkflowStep.ProvisionIdentity
            }),
            CreatedAt = DateTimeOffset.UtcNow,
            NextAttemptAt = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }

    public Task PublishRejectedAsync(TicketRejectedEvent ticketRejectedEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ticketRejectedEvent);

        var eventKey = $"ticket:{ticketRejectedEvent.TicketId}:rejected";
        if (_ticketDbContext.TicketWorkflowOutboxMessages.Local.Any(message => message.EventKey == eventKey))
        {
            return Task.CompletedTask;
        }

        _ticketDbContext.TicketWorkflowOutboxMessages.Add(new TicketWorkflowOutboxMessage
        {
            TicketId = ticketRejectedEvent.TicketId,
            EventKey = eventKey,
            EventType = TicketWorkflowOutboxEventTypes.Rejected,
            Payload = TicketWorkflowPayloadSerializer.Serialize(new TicketRejectedWorkflowPayload
            {
                TicketId = ticketRejectedEvent.TicketId,
                CurrentStep = TicketRejectedWorkflowStep.PublishRejectedEmail
            }),
            CreatedAt = DateTimeOffset.UtcNow,
            NextAttemptAt = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }
}
