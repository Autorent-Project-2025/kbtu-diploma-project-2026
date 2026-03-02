using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class IdentityProvisioningClient : IIdentityProvisioningClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly IdentityServiceOptions _identityServiceOptions;

    public IdentityProvisioningClient(
        HttpClient httpClient,
        IOptions<IdentityServiceOptions> identityServiceOptions)
    {
        _httpClient = httpClient;
        _identityServiceOptions = identityServiceOptions.Value;
    }

    public async Task<ProvisionIdentityUserResult> ProvisionUserAsync(
        ProvisionIdentityUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/internal/users/provision")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(InternalApiKeyHeader, _identityServiceOptions.InternalApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadFromJsonAsync<ProvisionIdentityUserResult>(cancellationToken: cancellationToken);
        if (responseBody is null)
        {
            throw new InvalidOperationException("Identity service returned an empty response body.");
        }

        return responseBody;
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Identity service request failed with status code {(int)response.StatusCode}.";

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
