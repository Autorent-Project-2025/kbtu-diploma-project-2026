using System.Text.Json;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Integrations;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Services
{
    public sealed class PendingBookingExpirationDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingBookingExpirationDispatcher> _logger;
        private readonly PendingBookingExpirationOptions _options;

        public PendingBookingExpirationDispatcher(
            IServiceScopeFactory scopeFactory,
            IOptions<PendingBookingExpirationOptions> options,
            ILogger<PendingBookingExpirationDispatcher> logger)
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
                    await ExpirePendingBookingsAsync(stoppingToken);
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task ExpirePendingBookingsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTimeOffset.UtcNow;
                var cutoff = now.AddMinutes(-_options.TtlMinutes);

                var candidateIds = await db.Bookings
                    .AsNoTracking()
                    .Where(booking => booking.Status == BookingStatus.Pending && booking.CreatedAt <= cutoff)
                    .OrderBy(booking => booking.Id)
                    .Take(_options.BatchSize)
                    .Select(booking => booking.Id)
                    .ToListAsync(cancellationToken);

                if (candidateIds.Count == 0)
                {
                    return;
                }

                var expiredCount = 0;

                foreach (var bookingId in candidateIds)
                {
                    var expired = await TryExpireBookingAsync(db, bookingId, cutoff, now, cancellationToken);
                    if (expired)
                    {
                        expiredCount += 1;
                    }
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync(cancellationToken);
                }

                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Expired {ExpiredPendingBookingCount} pending bookings older than {Cutoff}.",
                        expiredCount,
                        cutoff);
                }
            }
        }

        private static async Task<bool> TryExpireBookingAsync(
            ApplicationDbContext db,
            int bookingId,
            DateTimeOffset cutoff,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var affectedRows = db.Database.IsRelational()
                ? await db.Bookings
                    .Where(booking =>
                        booking.Id == bookingId &&
                        booking.Status == BookingStatus.Pending &&
                        booking.CreatedAt <= cutoff)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(booking => booking.Status, BookingStatus.Canceled),
                        cancellationToken)
                : await ExpireInMemoryAsync(db, bookingId, cutoff, cancellationToken);

            if (affectedRows == 0)
            {
                return false;
            }

            var eventKey = BuildCanceledEventKey(bookingId);
            var outboxExists = await db.PaymentSyncOutboxMessages
                .AnyAsync(message => message.EventKey == eventKey, cancellationToken);

            if (!outboxExists)
            {
                db.PaymentSyncOutboxMessages.Add(new PaymentSyncOutboxMessage
                {
                    BookingId = bookingId,
                    EventKey = eventKey,
                    EventType = PaymentSyncOutboxEventTypes.BookingCanceled,
                    Payload = JsonSerializer.Serialize(new PaymentSyncOutboxPayload
                    {
                        BookingId = bookingId
                    }),
                    CreatedAt = now,
                    NextAttemptAt = now
                });
            }

            return true;
        }

        private static async Task<int> ExpireInMemoryAsync(
            ApplicationDbContext db,
            int bookingId,
            DateTimeOffset cutoff,
            CancellationToken cancellationToken)
        {
            var booking = await db.Bookings
                .FirstOrDefaultAsync(item =>
                    item.Id == bookingId &&
                    item.Status == BookingStatus.Pending &&
                    item.CreatedAt <= cutoff,
                    cancellationToken);

            if (booking is null)
            {
                return 0;
            }

            booking.Status = BookingStatus.Canceled;
            return 1;
        }

        private static string BuildCanceledEventKey(int bookingId)
        {
            return $"booking:{bookingId}:status:{BookingStatus.Canceled.ToString().ToLowerInvariant()}";
        }
    }
}
