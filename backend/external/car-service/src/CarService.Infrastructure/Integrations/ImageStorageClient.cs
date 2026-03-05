using System.Net;
using System.Net.Http.Json;
using CarService.Application.Interfaces.Integrations;

namespace CarService.Infrastructure.Integrations
{
    public sealed class ImageStorageClient : IImageStorageClient
    {
        public ImageStorageClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;

        public async Task<ImageStorageUploadResult> UploadAsync(
            byte[] content,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            using var requestContent = new ByteArrayContent(content);
            requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            using var message = new HttpRequestMessage(HttpMethod.Post, "/api/images")
            {
                Content = requestContent
            };
            message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Image service upload failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<ImageServiceUploadResponse>(cancellationToken: cancellationToken);
            if (payload is null || string.IsNullOrWhiteSpace(payload.ImageId) || string.IsNullOrWhiteSpace(payload.ImageUrl))
            {
                throw new InvalidOperationException("Image service returned invalid upload response.");
            }

            return new ImageStorageUploadResult(payload.ImageId, payload.ImageUrl);
        }

        public async Task DeleteAsync(
            string imageId,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/images/{Uri.EscapeDataString(imageId)}");
            message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Image service delete failed with status {(int)response.StatusCode}."
                        : raw);
            }
        }

        private sealed class ImageServiceUploadResponse
        {
            public string ImageId { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
        }
    }
}
