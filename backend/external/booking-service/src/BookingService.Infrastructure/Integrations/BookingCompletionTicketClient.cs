using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Infrastructure.Integrations;

public sealed class BookingCompletionTicketClient : IBookingCompletionTicketClient
{
    private readonly HttpClient _httpClient;

    public BookingCompletionTicketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookingCompletionTicketPayload> CreateBookingCompletionTicketAsync(
        BookingCompletionTicketCreatePayload payload,
        CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'TicketService:BaseUrl' is required.");
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("BookingCompletion"), "ticketType");
        content.Add(new StringContent(payload.FirstName), "firstName");
        content.Add(new StringContent(payload.LastName), "lastName");
        content.Add(new StringContent(payload.Email), "email");
        content.Add(new StringContent(payload.PhoneNumber), "phoneNumber");
        content.Add(new StringContent(payload.BookingId.ToString()), "bookingId");
        content.Add(new StringContent(payload.PlannedStartTime.ToString("O")), "plannedStartTime");
        content.Add(new StringContent(payload.PlannedEndTime.ToString("O")), "plannedEndTime");
        content.Add(new StringContent(payload.TripStartedAt.ToString("O")), "tripStartedAt");
        content.Add(new StringContent(payload.TripCompletedAt.ToString("O")), "tripCompletedAt");

        if (payload.LatePenaltyAmount.HasValue && payload.LatePenaltyAmount.Value > 0m)
        {
            content.Add(new StringContent(payload.LatePenaltyAmount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "latePenaltyAmount");
        }

        AddFileContent(content, payload.CompletionFrontPhotoFile, "completionFrontPhotoFile");
        AddFileContent(content, payload.CompletionBackPhotoFile, "completionBackPhotoFile");
        AddFileContent(content, payload.CompletionSideLeftPhotoFile, "completionSideLeftPhotoFile");
        AddFileContent(content, payload.CompletionSideRightPhotoFile, "completionSideRightPhotoFile");
        AddFileContent(content, payload.CompletionInteriorPhotoFile, "completionInteriorPhotoFile");

        if (payload.DamageAssessment is not null)
        {
            // ticket-service deserialises this form field as a JSON blob
            // and writes it into the polymorphic Data column. Sending as
            // a string avoids multi-field explosion on the multipart
            // request and keeps the ticket API contract versioned.
            var assessmentJson = JsonSerializer.Serialize(
                payload.DamageAssessment,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            content.Add(new StringContent(assessmentJson, Encoding.UTF8, "application/json"), "damageAssessment");
        }

        using var response = await _httpClient.PostAsync("/", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Ticket service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        var body = await response.Content.ReadFromJsonAsync<BookingCompletionTicketPayload>(cancellationToken: cancellationToken);
        return body ?? throw new InvalidOperationException("Ticket service response body is invalid.");
    }

    private static void AddFileContent(MultipartFormDataContent formData, FileUploadPayload file, string fieldName)
    {
        var fileContent = new ByteArrayContent(file.Content);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
        }

        formData.Add(fileContent, fieldName, file.FileName);
    }
}
