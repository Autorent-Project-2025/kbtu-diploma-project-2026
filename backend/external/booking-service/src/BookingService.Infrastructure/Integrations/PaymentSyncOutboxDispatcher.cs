using System.Text.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class PaymentSyncOutboxDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentSyncOutboxDispatcher> _logger;
        private readonly PaymentSyncOutboxOptions _options;

        public PaymentSyncOutboxDispatcher(
            IServiceScopeFactory scopeFactory,
            IOptions<PaymentSyncOutboxOptions> options,
            ILogger<PaymentSyncOutboxDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
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
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var paymentSyncClient = scope.ServiceProvider.GetRequiredService<IPaymentSyncClient>();

                var batch = await ClaimBatchAsync(db, cancellationToken);
                if (batch.Count == 0)
                {
                    return;
                }

                foreach (var message in batch)
                {
                    await DispatchMessageAsync(db, paymentSyncClient, message, cancellationToken);
                }
            }
        }

        private async Task<List<PaymentSyncOutboxMessage>> ClaimBatchAsync(ApplicationDbContext db, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var lockUntil = now.AddSeconds(_options.LockTimeoutSeconds);

            var messages = await db.PaymentSyncOutboxMessages
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
            ApplicationDbContext db,
            IPaymentSyncClient paymentSyncClient,
            PaymentSyncOutboxMessage message,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            try
            {
                var payload = DeserializePayload(message.Payload);
                await SendAsync(paymentSyncClient, message.EventType, payload, cancellationToken);

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
                    "Payment outbox dispatch failed for message {OutboxMessageId} ({EventType}). Attempt {AttemptCount}.",
                    message.Id,
                    message.EventType,
                    message.AttemptCount);
            }
        }

        private static PaymentSyncOutboxPayload DeserializePayload(string payload)
        {
            var model = JsonSerializer.Deserialize<PaymentSyncOutboxPayload>(payload);
            if (model is null)
            {
                throw new InvalidOperationException("Payment outbox payload is invalid.");
            }

            return model;
        }

        private static async Task SendAsync(
            IPaymentSyncClient paymentSyncClient,
            string eventType,
            PaymentSyncOutboxPayload payload,
            CancellationToken cancellationToken)
        {
            switch (eventType)
            {
                case PaymentSyncOutboxEventTypes.BookingConfirmed:
                    if (payload.UserId is null || payload.PartnerUserId is null || payload.PartnerCarId is null)
                    {
                        throw new InvalidOperationException("Booking confirmed outbox payload is incomplete.");
                    }

                    await paymentSyncClient.RecordBookingConfirmedAsync(
                        payload.BookingId,
                        payload.UserId.Value,
                        payload.PartnerUserId.Value,
                        payload.PartnerCarId.Value,
                        payload.PriceHour,
                        payload.TotalPrice,
                        cancellationToken);
                    return;

                case PaymentSyncOutboxEventTypes.BookingCanceled:
                    await paymentSyncClient.RecordBookingCanceledAsync(payload.BookingId, cancellationToken);
                    return;

                case PaymentSyncOutboxEventTypes.BookingCompleted:
                    await paymentSyncClient.RecordBookingCompletedAsync(payload.BookingId, cancellationToken);
                    return;

                default:
                    throw new InvalidOperationException($"Unsupported payment outbox event type '{eventType}'.");
            }
        }

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
    }
}
