using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Infrastructure.Integrations;

public sealed class PartnerBookingCancellationTicketClient : IPartnerBookingCancellationTicketClient
{
    private readonly HttpClient _httpClient;

    public PartnerBookingCancellationTicketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PartnerBookingCancellationTicketPayload> CreatePartnerBookingCancellationTicketAsync(
        PartnerBookingCancellationTicketCreatePayload payload,
        CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'TicketService:BaseUrl' is required.");
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("PartnerBookingCancellation"), "ticketType");
        content.Add(new StringContent(payload.FirstName), "firstName");
        content.Add(new StringContent(payload.LastName), "lastName");
        content.Add(new StringContent(payload.Email), "email");
        content.Add(new StringContent(payload.PhoneNumber), "phoneNumber");
        content.Add(new StringContent(payload.RelatedPartnerUserId.ToString("D")), "relatedPartnerUserId");
        content.Add(new StringContent(payload.BookingId.ToString()), "bookingId");
        content.Add(new StringContent(payload.CarBrand), "carBrand");
        content.Add(new StringContent(payload.CarModel), "carModel");
        content.Add(new StringContent(payload.BookingStatus), "bookingStatus");
        content.Add(new StringContent(payload.BookingStartTime.ToString("O")), "bookingStartTime");
        content.Add(new StringContent(payload.BookingEndTime.ToString("O")), "bookingEndTime");
        content.Add(new StringContent(payload.PartnerReason), "partnerReason");

        using var response = await _httpClient.PostAsync("/", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Ticket service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        var body = await response.Content.ReadFromJsonAsync<PartnerBookingCancellationTicketPayload>(cancellationToken: cancellationToken);
        return body ?? throw new InvalidOperationException("Ticket service response body is invalid.");
    }
}
