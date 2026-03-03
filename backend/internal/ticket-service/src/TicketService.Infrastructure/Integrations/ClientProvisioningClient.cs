using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class ClientProvisioningClient : IClientProvisioningClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly ClientServiceOptions _clientServiceOptions;

    public ClientProvisioningClient(
        HttpClient httpClient,
        IOptions<ClientServiceOptions> clientServiceOptions)
    {
        _httpClient = httpClient;
        _clientServiceOptions = clientServiceOptions.Value;
    }

    public async Task ProvisionClientAsync(
        ProvisionClientProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/internal/clients/provision")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(InternalApiKeyHeader, _clientServiceOptions.InternalApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Client service request failed with status code {(int)response.StatusCode}.";

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
}
