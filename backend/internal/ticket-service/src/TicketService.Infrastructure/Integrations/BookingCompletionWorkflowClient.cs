using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class BookingCompletionWorkflowClient : IBookingCompletionWorkflowClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly BookingServiceOptions _bookingServiceOptions;

    public BookingCompletionWorkflowClient(
        HttpClient httpClient,
        IOptions<BookingServiceOptions> bookingServiceOptions)
    {
        _httpClient = httpClient;
        _bookingServiceOptions = bookingServiceOptions.Value;
    }

    public Task ApproveCompletionReviewAsync(
        ApproveBookingCompletionReviewWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Post,
            $"/internal/bookings/{request.BookingId}/completion-review/approve",
            new
            {
                request.TicketId,
                request.LatePenaltyAmount,
                request.CustomerEmail,
                request.CustomerFullName
            },
            cancellationToken);
    }

    public Task IssueCompletionFineAsync(
        IssueBookingCompletionFineWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Post,
            $"/internal/bookings/{request.BookingId}/completion-review/fine-issued",
            new
            {
                request.TicketId,
                request.LatePenaltyAmount,
                request.DamageFineAmount,
                request.FineComment,
                request.CustomerEmail,
                request.CustomerFullName
            },
            cancellationToken);
    }

    private async Task SendAsync(
        HttpMethod method,
        string path,
        object payload,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload)
        };
        message.Headers.Add(InternalApiKeyHeader, _bookingServiceOptions.InternalApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Booking service request failed with status code {(int)response.StatusCode}.";

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ValidationException(errorMessage),
            HttpStatusCode.Conflict => new ConflictException(errorMessage),
            HttpStatusCode.NotFound => new NotFoundException(errorMessage),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new UnauthorizedException(errorMessage),
            _ => new InvalidOperationException(errorMessage)
        };
    }

    private static async Task<string?> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(rawContent);
            if (jsonDocument.RootElement.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString();
            }

            if (jsonDocument.RootElement.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString();
            }

            if (jsonDocument.RootElement.TryGetProperty("detail", out var detailProperty))
            {
                return detailProperty.GetString();
            }
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }
}
