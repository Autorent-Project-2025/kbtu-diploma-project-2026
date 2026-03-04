using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Infrastructure.Integrations;

public sealed class EmailNotificationClient : IEmailNotificationClient
{
    private readonly HttpClient _httpClient;

    public EmailNotificationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendApprovedAsync(
        SendApprovedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/approved", new
        {
            to = request.To,
            fullName = request.FullName,
            loginEmail = request.LoginEmail,
            setPasswordUrl = request.SetPasswordUrl
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    public async Task SendRejectedAsync(
        SendRejectedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/rejected", new
        {
            to = request.To,
            fullName = request.FullName,
            reason = request.Reason
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    public async Task SendPartnerApprovedAsync(
        SendPartnerApprovedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/partners/approved", new
        {
            to = request.To,
            fullName = request.FullName,
            loginEmail = request.LoginEmail,
            setPasswordUrl = request.SetPasswordUrl
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    public async Task SendPartnerRejectedAsync(
        SendPartnerRejectedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/partners/rejected", new
        {
            to = request.To,
            fullName = request.FullName,
            reason = request.Reason
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    public async Task SendPartnerCarApprovedAsync(
        SendPartnerCarApprovedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/partners/cars/approved", new
        {
            to = request.To,
            fullName = request.FullName,
            carBrand = request.CarBrand,
            carModel = request.CarModel,
            licensePlate = request.LicensePlate
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    public async Task SendPartnerCarRejectedAsync(
        SendPartnerCarRejectedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/emails/partners/cars/rejected", new
        {
            to = request.To,
            fullName = request.FullName,
            carBrand = request.CarBrand,
            carModel = request.CarModel,
            licensePlate = request.LicensePlate,
            reason = request.Reason
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Email service request failed with status code {(int)response.StatusCode}.";

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ValidationException(errorMessage),
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
