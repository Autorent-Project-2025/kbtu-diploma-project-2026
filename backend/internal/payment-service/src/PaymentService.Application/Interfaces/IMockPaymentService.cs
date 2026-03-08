using PaymentService.Application.DTOs;
using PaymentService.Application.Models;

namespace PaymentService.Application.Interfaces;

public interface IMockPaymentService
{
    Task<MockPaymentAttemptResponseDto> StartAsync(
        StartMockPaymentCommand command,
        CancellationToken cancellationToken = default);

    Task<MockPaymentAttemptResponseDto?> GetLatestAsync(
        int bookingId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<MockPaymentAttemptResponseDto> SubmitAsync(
        SubmitMockPaymentCommand command,
        CancellationToken cancellationToken = default);
}
