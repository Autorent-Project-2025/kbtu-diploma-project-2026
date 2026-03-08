using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Options;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Services;

public sealed class PaymentLedgerService : IPaymentLedgerService
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentOptions _options;

    public PaymentLedgerService(ApplicationDbContext db, IOptions<PaymentOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task RecordBookingConfirmedAsync(BookingPaymentSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ValidateSnapshot(snapshot);

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var existingPayment = await _db.CustomerPayments
            .FirstOrDefaultAsync(payment => payment.BookingId == snapshot.BookingId, cancellationToken);

        if (existingPayment is not null)
        {
            if (existingPayment.Status is CustomerPaymentStatus.Pending or CustomerPaymentStatus.Available)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            throw new InvalidOperationException("Booking payment is already canceled and cannot be confirmed again.");
        }

        var now = DateTimeOffset.UtcNow;
        var currency = NormalizeCurrency(_options.Currency);
        var commissionRate = NormalizeCommissionRate(_options.PlatformCommissionRate);
        var grossAmount = NormalizeAmount(snapshot.TotalPrice ?? 0m);
        var platformCommissionAmount = decimal.Round(grossAmount * commissionRate, 2, MidpointRounding.AwayFromZero);
        var partnerAmount = decimal.Round(grossAmount - platformCommissionAmount, 2, MidpointRounding.AwayFromZero);

        var wallet = await GetOrCreateWalletAsync(snapshot.PartnerUserId, currency, now, cancellationToken);
        wallet.PendingAmount = decimal.Round(wallet.PendingAmount + partnerAmount, 2, MidpointRounding.AwayFromZero);
        wallet.UpdatedAt = now;

        var payment = new CustomerPayment
        {
            BookingId = snapshot.BookingId,
            UserId = snapshot.UserId,
            PartnerUserId = snapshot.PartnerUserId,
            PartnerCarId = snapshot.PartnerCarId,
            PriceHour = snapshot.PriceHour,
            GrossAmount = grossAmount,
            PlatformCommissionRate = commissionRate,
            PlatformCommissionAmount = platformCommissionAmount,
            PartnerAmount = partnerAmount,
            Currency = currency,
            Status = CustomerPaymentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            ConfirmedAt = now
        };

        _db.CustomerPayments.Add(payment);
        _db.PartnerLedgerEntries.Add(new PartnerLedgerEntry
        {
            PartnerWallet = wallet,
            CustomerPayment = payment,
            BookingId = snapshot.BookingId,
            EntryType = LedgerEntryType.BookingPendingCredit,
            Bucket = LedgerBucket.Pending,
            AmountDelta = partnerAmount,
            Currency = currency,
            Description = $"Booking {snapshot.BookingId} confirmed.",
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RecordBookingCanceledAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        if (bookingId <= 0)
        {
            throw new ArgumentException("Booking id must be greater than zero.", nameof(bookingId));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var payment = await _db.CustomerPayments
            .FirstOrDefaultAsync(item => item.BookingId == bookingId, cancellationToken);

        if (payment is null || payment.Status == CustomerPaymentStatus.Canceled)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (payment.Status == CustomerPaymentStatus.Available)
        {
            throw new InvalidOperationException("Completed booking payment cannot be canceled.");
        }

        var wallet = await GetRequiredWalletAsync(payment.PartnerUserId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        wallet.PendingAmount = EnsureNonNegative(wallet.PendingAmount - payment.PartnerAmount, "pending amount");
        wallet.UpdatedAt = now;

        payment.Status = CustomerPaymentStatus.Canceled;
        payment.CanceledAt = now;
        payment.UpdatedAt = now;

        _db.PartnerLedgerEntries.Add(new PartnerLedgerEntry
        {
            PartnerWalletId = wallet.Id,
            CustomerPaymentId = payment.Id,
            BookingId = payment.BookingId,
            EntryType = LedgerEntryType.BookingPendingReversal,
            Bucket = LedgerBucket.Pending,
            AmountDelta = decimal.Round(-payment.PartnerAmount, 2, MidpointRounding.AwayFromZero),
            Currency = payment.Currency,
            Description = $"Booking {payment.BookingId} canceled.",
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RecordBookingCompletedAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        if (bookingId <= 0)
        {
            throw new ArgumentException("Booking id must be greater than zero.", nameof(bookingId));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var payment = await _db.CustomerPayments
            .FirstOrDefaultAsync(item => item.BookingId == bookingId, cancellationToken);

        if (payment is null)
        {
            throw new KeyNotFoundException($"Customer payment for booking {bookingId} was not found.");
        }

        if (payment.Status == CustomerPaymentStatus.Available)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (payment.Status == CustomerPaymentStatus.Canceled)
        {
            throw new InvalidOperationException("Canceled booking payment cannot be completed.");
        }

        var wallet = await GetRequiredWalletAsync(payment.PartnerUserId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        wallet.PendingAmount = EnsureNonNegative(wallet.PendingAmount - payment.PartnerAmount, "pending amount");
        wallet.AvailableAmount = decimal.Round(wallet.AvailableAmount + payment.PartnerAmount, 2, MidpointRounding.AwayFromZero);
        wallet.UpdatedAt = now;

        payment.Status = CustomerPaymentStatus.Available;
        payment.AvailableAt = now;
        payment.UpdatedAt = now;

        _db.PartnerLedgerEntries.AddRange(
            new PartnerLedgerEntry
            {
                PartnerWalletId = wallet.Id,
                CustomerPaymentId = payment.Id,
                BookingId = payment.BookingId,
                EntryType = LedgerEntryType.BookingPendingRelease,
                Bucket = LedgerBucket.Pending,
                AmountDelta = decimal.Round(-payment.PartnerAmount, 2, MidpointRounding.AwayFromZero),
                Currency = payment.Currency,
                Description = $"Booking {payment.BookingId} completed: pending release.",
                CreatedAt = now
            },
            new PartnerLedgerEntry
            {
                PartnerWalletId = wallet.Id,
                CustomerPaymentId = payment.Id,
                BookingId = payment.BookingId,
                EntryType = LedgerEntryType.BookingAvailableCredit,
                Bucket = LedgerBucket.Available,
                AmountDelta = payment.PartnerAmount,
                Currency = payment.Currency,
                Description = $"Booking {payment.BookingId} completed: available credit.",
                CreatedAt = now
            });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<PartnerWalletResponseDto?> GetWalletAsync(Guid partnerUserId, CancellationToken cancellationToken = default)
    {
        if (partnerUserId == Guid.Empty)
        {
            throw new ArgumentException("Partner user id is required.", nameof(partnerUserId));
        }

        return await _db.PartnerWallets
            .AsNoTracking()
            .Where(wallet => wallet.PartnerUserId == partnerUserId)
            .Select(wallet => new PartnerWalletResponseDto
            {
                PartnerUserId = wallet.PartnerUserId,
                Currency = wallet.Currency,
                PendingAmount = wallet.PendingAmount,
                AvailableAmount = wallet.AvailableAmount,
                ReservedAmount = wallet.ReservedAmount,
                UpdatedAt = wallet.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartnerLedgerEntryResponseDto>> GetLedgerAsync(Guid partnerUserId, int take, CancellationToken cancellationToken = default)
    {
        if (partnerUserId == Guid.Empty)
        {
            throw new ArgumentException("Partner user id is required.", nameof(partnerUserId));
        }

        var normalizedTake = Math.Clamp(take, 1, 200);

        return await _db.PartnerLedgerEntries
            .AsNoTracking()
            .Where(entry => entry.PartnerWallet.PartnerUserId == partnerUserId)
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .Take(normalizedTake)
            .Select(entry => new PartnerLedgerEntryResponseDto
            {
                Id = entry.Id,
                PartnerUserId = entry.PartnerWallet.PartnerUserId,
                BookingId = entry.BookingId,
                CustomerPaymentId = entry.CustomerPaymentId,
                PartnerPayoutId = entry.PartnerPayoutId,
                EntryType = entry.EntryType.ToString(),
                Bucket = entry.Bucket.ToString(),
                AmountDelta = entry.AmountDelta,
                Currency = entry.Currency,
                Description = entry.Description,
                CreatedAt = entry.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<PartnerWallet> GetOrCreateWalletAsync(Guid partnerUserId, string currency, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var wallet = await _db.PartnerWallets
            .FirstOrDefaultAsync(item => item.PartnerUserId == partnerUserId, cancellationToken);

        if (wallet is not null)
        {
            if (!string.Equals(wallet.Currency, currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Partner wallet currency mismatch.");
            }

            return wallet;
        }

        wallet = new PartnerWallet
        {
            PartnerUserId = partnerUserId,
            Currency = currency,
            PendingAmount = 0m,
            AvailableAmount = 0m,
            ReservedAmount = 0m,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.PartnerWallets.Add(wallet);
        return wallet;
    }

    private async Task<PartnerWallet> GetRequiredWalletAsync(Guid partnerUserId, CancellationToken cancellationToken)
    {
        var wallet = await _db.PartnerWallets
            .FirstOrDefaultAsync(item => item.PartnerUserId == partnerUserId, cancellationToken);

        if (wallet is null)
        {
            throw new KeyNotFoundException($"Partner wallet for user {partnerUserId} was not found.");
        }

        return wallet;
    }

    private static void ValidateSnapshot(BookingPaymentSnapshot snapshot)
    {
        if (snapshot.BookingId <= 0)
        {
            throw new ArgumentException("Booking id must be greater than zero.", nameof(snapshot.BookingId));
        }

        if (snapshot.UserId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(snapshot.UserId));
        }

        if (snapshot.PartnerUserId == Guid.Empty)
        {
            throw new ArgumentException("Partner user id is required.", nameof(snapshot.PartnerUserId));
        }

        if (snapshot.PartnerCarId <= 0)
        {
            throw new ArgumentException("Partner car id must be greater than zero.", nameof(snapshot.PartnerCarId));
        }

        if (snapshot.PriceHour.HasValue && snapshot.PriceHour.Value < 0m)
        {
            throw new ArgumentException("PriceHour cannot be negative.", nameof(snapshot.PriceHour));
        }

        if (snapshot.TotalPrice.HasValue && snapshot.TotalPrice.Value < 0m)
        {
            throw new ArgumentException("TotalPrice cannot be negative.", nameof(snapshot.TotalPrice));
        }
    }

    private static decimal NormalizeAmount(decimal amount)
    {
        if (amount < 0m)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new InvalidOperationException("Payment currency configuration is required.");
        }

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
        {
            throw new InvalidOperationException("Payment currency must be a 3-letter code.");
        }

        return normalized;
    }

    private static decimal NormalizeCommissionRate(decimal rate)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new InvalidOperationException("Platform commission rate must be between 0 and 1.");
        }

        return decimal.Round(rate, 4, MidpointRounding.AwayFromZero);
    }

    private static decimal EnsureNonNegative(decimal amount, string bucketName)
    {
        var normalized = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (normalized < 0m)
        {
            throw new InvalidOperationException($"Partner wallet {bucketName} cannot become negative.");
        }

        return normalized;
    }
}
