using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Services;

public sealed class MockPaymentService : IMockPaymentService
{
    private const int SessionTtlMinutes = 15;
    private const string SuccessCardNumber = "4242424242424242";
    private const string DeclinedCardNumber = "4000000000000002";
    private const string InsufficientFundsCardNumber = "4000000000009995";

    private readonly ApplicationDbContext _db;

    public MockPaymentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MockPaymentAttemptResponseDto> StartAsync(
        StartMockPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateStartCommand(command);

        var now = DateTimeOffset.UtcNow;
        var currency = NormalizeCurrency(command.Currency);
        var amount = NormalizeAmount(command.Amount);

        var latestAttempt = await _db.MockPaymentAttempts
            .Where(attempt => attempt.BookingId == command.BookingId && attempt.UserId == command.UserId)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ThenByDescending(attempt => attempt.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestAttempt is not null)
        {
            var wasExpired = MarkExpiredIfNeeded(latestAttempt, now);
            if (wasExpired)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            if (latestAttempt.Status is MockPaymentAttemptStatus.Started or MockPaymentAttemptStatus.Succeeded)
            {
                return Map(latestAttempt);
            }
        }

        var attempt = new MockPaymentAttempt
        {
            BookingId = command.BookingId,
            UserId = command.UserId,
            SessionKey = Guid.NewGuid().ToString("N"),
            Amount = amount,
            Currency = currency,
            Status = MockPaymentAttemptStatus.Started,
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = now.AddMinutes(SessionTtlMinutes)
        };

        _db.MockPaymentAttempts.Add(attempt);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(attempt);
    }

    public async Task<MockPaymentAttemptResponseDto?> GetLatestAsync(
        int bookingId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateIdentity(bookingId, userId);

        var attempt = await _db.MockPaymentAttempts
            .Where(item => item.BookingId == bookingId && item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt is null)
        {
            return null;
        }

        if (MarkExpiredIfNeeded(attempt, DateTimeOffset.UtcNow))
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Map(attempt);
    }

    public async Task<MockPaymentAttemptResponseDto> SubmitAsync(
        SubmitMockPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateSubmitCommand(command);

        var attempt = await _db.MockPaymentAttempts
            .FirstOrDefaultAsync(item =>
                item.BookingId == command.BookingId &&
                item.UserId == command.UserId &&
                item.SessionKey == command.SessionKey,
                cancellationToken);

        if (attempt is null)
        {
            throw new KeyNotFoundException($"Mock payment attempt for booking {command.BookingId} was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        if (MarkExpiredIfNeeded(attempt, now))
        {
            await _db.SaveChangesAsync(cancellationToken);
            return Map(attempt);
        }

        if (attempt.Status != MockPaymentAttemptStatus.Started)
        {
            return Map(attempt);
        }

        var cardHolder = NormalizeCardHolder(command.CardHolder);
        var cardNumber = NormalizeCardNumber(command.CardNumber);
        var cvv = NormalizeCvv(command.Cvv);
        ValidateExpiry(command.ExpiryMonth, command.ExpiryYear, now);

        _ = cvv;

        attempt.CardHolder = cardHolder;
        attempt.CardLast4 = cardNumber[^4..];
        attempt.UpdatedAt = now;
        attempt.CompletedAt = now;

        var failureReason = cardNumber switch
        {
            DeclinedCardNumber => "Card was declined.",
            InsufficientFundsCardNumber => "Insufficient funds.",
            _ => null
        };

        if (failureReason is null || cardNumber == SuccessCardNumber)
        {
            attempt.Status = MockPaymentAttemptStatus.Succeeded;
            attempt.FailureReason = null;
        }
        else
        {
            attempt.Status = MockPaymentAttemptStatus.Failed;
            attempt.FailureReason = failureReason;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Map(attempt);
    }

    private static void ValidateStartCommand(StartMockPaymentCommand command)
    {
        ValidateIdentity(command.BookingId, command.UserId);
        _ = NormalizeAmount(command.Amount);
        _ = NormalizeCurrency(command.Currency);
    }

    private static void ValidateSubmitCommand(SubmitMockPaymentCommand command)
    {
        ValidateIdentity(command.BookingId, command.UserId);

        if (string.IsNullOrWhiteSpace(command.SessionKey))
        {
            throw new ArgumentException("Mock payment session key is required.", nameof(command.SessionKey));
        }
    }

    private static void ValidateIdentity(int bookingId, Guid userId)
    {
        if (bookingId <= 0)
        {
            throw new ArgumentException("Booking id must be greater than zero.", nameof(bookingId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }
    }

    private static bool MarkExpiredIfNeeded(MockPaymentAttempt attempt, DateTimeOffset now)
    {
        if (attempt.Status != MockPaymentAttemptStatus.Started || attempt.ExpiresAt > now)
        {
            return false;
        }

        attempt.Status = MockPaymentAttemptStatus.Expired;
        attempt.UpdatedAt = now;
        attempt.CompletedAt = now;
        attempt.FailureReason = "Mock payment session expired.";
        return true;
    }

    private static decimal NormalizeAmount(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));
        }

        return normalized;
    }

    private static string NormalizeCardHolder(string cardHolder)
    {
        if (string.IsNullOrWhiteSpace(cardHolder))
        {
            throw new ArgumentException("Card holder is required.", nameof(cardHolder));
        }

        var normalized = cardHolder.Trim();
        if (normalized.Length > 128)
        {
            throw new ArgumentException("Card holder length must not exceed 128 characters.", nameof(cardHolder));
        }

        return normalized;
    }

    private static string NormalizeCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new ArgumentException("Card number is required.", nameof(cardNumber));
        }

        var normalized = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (normalized.Length != 16)
        {
            throw new ArgumentException("Card number must contain 16 digits.", nameof(cardNumber));
        }

        return normalized;
    }

    private static string NormalizeCvv(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
        {
            throw new ArgumentException("CVV is required.", nameof(cvv));
        }

        var normalized = new string(cvv.Where(char.IsDigit).ToArray());
        if (normalized.Length is < 3 or > 4)
        {
            throw new ArgumentException("CVV must contain 3 or 4 digits.", nameof(cvv));
        }

        return normalized;
    }

    private static void ValidateExpiry(int expiryMonth, int expiryYear, DateTimeOffset now)
    {
        if (expiryMonth is < 1 or > 12)
        {
            throw new ArgumentException("Expiry month must be between 1 and 12.", nameof(expiryMonth));
        }

        if (expiryYear < now.Year || expiryYear > now.Year + 20)
        {
            throw new ArgumentException("Expiry year is invalid.", nameof(expiryYear));
        }

        if (expiryYear == now.Year && expiryMonth < now.Month)
        {
            throw new ArgumentException("Card expiry date has passed.", nameof(expiryMonth));
        }
    }

    private static MockPaymentAttemptResponseDto Map(MockPaymentAttempt attempt)
    {
        return new MockPaymentAttemptResponseDto
        {
            Id = attempt.Id,
            BookingId = attempt.BookingId,
            UserId = attempt.UserId,
            SessionKey = attempt.SessionKey,
            Amount = attempt.Amount,
            Currency = attempt.Currency,
            Status = attempt.Status.ToString().ToLowerInvariant(),
            CardHolder = attempt.CardHolder,
            CardLast4 = attempt.CardLast4,
            FailureReason = attempt.FailureReason,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt,
            CompletedAt = attempt.CompletedAt,
            ExpiresAt = attempt.ExpiresAt
        };
    }
}
