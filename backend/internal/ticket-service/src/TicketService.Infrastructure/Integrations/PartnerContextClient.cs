using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Infrastructure.Integrations;

public sealed class PartnerContextClient : IPartnerContextClient
{
    private readonly HttpClient _httpClient;

    public PartnerContextClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PartnerContextResult?> GetCurrentPartnerAsync(
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/me");
        message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<PartnerMeResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException("Partner service returned empty response.");
        }

        if (!Guid.TryParse(payload.RelatedUserId, out var relatedUserId))
        {
            throw new ValidationException("Partner service returned invalid related user id.");
        }

        return new PartnerContextResult(
            payload.OwnerFirstName,
            payload.OwnerLastName,
            payload.PhoneNumber,
            relatedUserId);
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Partner service request failed with status code {(int)response.StatusCode}.";

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
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }

    private sealed class PartnerMeResponse
    {
        public string OwnerFirstName { get; init; } = string.Empty;
        public string OwnerLastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string RelatedUserId { get; init; } = string.Empty;
    }
}
