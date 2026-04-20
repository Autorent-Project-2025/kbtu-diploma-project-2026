using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class DamageEvaluationClient : IDamageEvaluationClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly DamageEvalServiceOptions _options;
        private readonly ILogger<DamageEvaluationClient> _logger;

        private static readonly JsonSerializerOptions ResponseJsonOptions = new(JsonSerializerDefaults.Web);

        public DamageEvaluationClient(
            HttpClient httpClient,
            IOptions<DamageEvalServiceOptions> options,
            ILogger<DamageEvaluationClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<DamageEvaluationAssessment> InspectSessionAsync(
            DamageEvaluationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("DamageEvalService:BaseUrl configuration is required.");
            }

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.PartnerCarId.ToString(System.Globalization.CultureInfo.InvariantCulture)), "car_id");
            form.Add(new StringContent(string.IsNullOrWhiteSpace(request.CarBrand) ? request.CarModel : $"{request.CarBrand} {request.CarModel}"), "car_model");
            form.Add(new StringContent(request.CarColor ?? string.Empty), "car_color");

            AddFile(form, request.FrontPhoto, "photo_front");
            AddFile(form, request.BackPhoto, "photo_back");
            AddFile(form, request.SideLeftPhoto, "photo_side_left");
            AddFile(form, request.SideRightPhoto, "photo_side_right");
            AddFile(form, request.InteriorPhoto, "photo_interior");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/inspect-session")
            {
                Content = form,
            };
            if (!string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                httpRequest.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            }

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.StatusCode >= HttpStatusCode.InternalServerError)
                {
                    _logger.LogWarning(
                        "Damage eval service responded with {StatusCode} for partner car {PartnerCarId}; falling open.",
                        (int)response.StatusCode,
                        request.PartnerCarId);
                    return UnavailableAssessment($"AI service returned {(int)response.StatusCode}.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Damage eval service returned client error {StatusCode} for partner car {PartnerCarId}: {Body}",
                        (int)response.StatusCode,
                        request.PartnerCarId,
                        errorBody);
                    return ErrorAssessment(
                        string.IsNullOrWhiteSpace(errorBody)
                            ? $"AI service returned {(int)response.StatusCode}."
                            : errorBody);
                }

                var payload = await response.Content.ReadFromJsonAsync<InspectionResponseBody>(
                    ResponseJsonOptions,
                    cancellationToken);
                if (payload is null)
                {
                    return ErrorAssessment("AI service returned empty response body.");
                }

                return MapSuccess(payload);
            }
            catch (TaskCanceledException)
            {
                // Propagate true cancellations from the caller. Only HTTP
                // timeouts — raised as TaskCanceledException without the
                // caller's token — become unavailable.
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                _logger.LogWarning("Damage eval service timed out for partner car {PartnerCarId}; falling open.", request.PartnerCarId);
                return UnavailableAssessment("AI service timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Damage eval service is unreachable for partner car {PartnerCarId}; falling open.", request.PartnerCarId);
                return UnavailableAssessment(ex.Message);
            }
        }

        private static DamageEvaluationAssessment MapSuccess(InspectionResponseBody body)
        {
            var verdict = body.Verdict switch
            {
                "OK" => DamageEvaluationVerdict.Ok,
                "DAMAGES_FOUND" => DamageEvaluationVerdict.DamagesFound,
                "INVALID_SESSION" => DamageEvaluationVerdict.InvalidSession,
                _ => (DamageEvaluationVerdict?)null,
            };

            if (verdict is null)
            {
                return ErrorAssessment($"AI service returned unexpected verdict '{body.Verdict}'.");
            }

            var damages = (body.Damages ?? new List<InspectionDamageBody>())
                .Select(item => new DamageEvaluationDamage(
                    item.Type ?? string.Empty,
                    item.Confidence,
                    (IReadOnlyList<int>?)item.Bbox ?? Array.Empty<int>(),
                    item.Slot,
                    item.SourceFile))
                .ToList();

            var rejected = (body.RejectedPhotos ?? new List<InspectionRejectedPhotoBody>())
                .Select(item => new DamageEvaluationRejectedPhoto(
                    item.Slot,
                    item.Filename ?? string.Empty,
                    item.Step,
                    item.Reason ?? string.Empty,
                    item.Details ?? new List<string>()))
                .ToList();

            var status = verdict == DamageEvaluationVerdict.InvalidSession
                ? DamageEvaluationStatus.InvalidSession
                : DamageEvaluationStatus.Ok;

            return new DamageEvaluationAssessment(
                status,
                verdict,
                body.ValidPhotosCount,
                damages,
                rejected,
                body.ProcessedAtUtc ?? DateTimeOffset.UtcNow,
                null);
        }

        private static DamageEvaluationAssessment UnavailableAssessment(string message) =>
            new(DamageEvaluationStatus.Unavailable, null, 0,
                Array.Empty<DamageEvaluationDamage>(),
                Array.Empty<DamageEvaluationRejectedPhoto>(),
                DateTimeOffset.UtcNow, message);

        private static DamageEvaluationAssessment ErrorAssessment(string message) =>
            new(DamageEvaluationStatus.Error, null, 0,
                Array.Empty<DamageEvaluationDamage>(),
                Array.Empty<DamageEvaluationRejectedPhoto>(),
                DateTimeOffset.UtcNow, message);

        private static void AddFile(MultipartFormDataContent form, FileUploadPayload file, string fieldName)
        {
            var content = new ByteArrayContent(file.Content);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            }

            form.Add(content, fieldName, string.IsNullOrWhiteSpace(file.FileName) ? $"{fieldName}.jpg" : file.FileName);
        }

        private sealed class InspectionResponseBody
        {
            [JsonPropertyName("verdict")] public string Verdict { get; set; } = string.Empty;
            [JsonPropertyName("damages")] public List<InspectionDamageBody>? Damages { get; set; }
            [JsonPropertyName("rejected_photos")] public List<InspectionRejectedPhotoBody>? RejectedPhotos { get; set; }
            [JsonPropertyName("valid_photos_count")] public int ValidPhotosCount { get; set; }
            [JsonPropertyName("processed_at_utc")] public DateTimeOffset? ProcessedAtUtc { get; set; }
        }

        private sealed class InspectionDamageBody
        {
            [JsonPropertyName("type")] public string? Type { get; set; }
            [JsonPropertyName("confidence")] public double Confidence { get; set; }
            [JsonPropertyName("bbox")] public List<int>? Bbox { get; set; }
            [JsonPropertyName("slot")] public string? Slot { get; set; }
            [JsonPropertyName("source_file")] public string? SourceFile { get; set; }
        }

        private sealed class InspectionRejectedPhotoBody
        {
            [JsonPropertyName("slot")] public string? Slot { get; set; }
            [JsonPropertyName("filename")] public string? Filename { get; set; }
            [JsonPropertyName("step")] public int Step { get; set; }
            [JsonPropertyName("reason")] public string? Reason { get; set; }
            [JsonPropertyName("details")] public List<string>? Details { get; set; }
        }
    }
}
