using BookingService.Domain.Enums;
using BookingService.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace BookingService.Domain.Entities
{
    public class Booking
    {
        private static readonly JsonSerializerOptions PricingBreakdownSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [NotMapped]
        private BookingPricingBreakdownSnapshot? _pricingBreakdown;

        [NotMapped]
        private List<string>? _imageUrls;

        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("partner_car_id")]
        public int PartnerCarId { get; set; }

        [Column("partner_user_id")]
        public Guid PartnerUserId { get; set; }

        [Column("car_brand")]
        public string? CarBrand { get; set; }

        [Column("car_model")]
        public string? CarModel { get; set; }

        [Column("partner_name")]
        public string? PartnerName { get; set; }

        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [Column("start_time")]
        public DateTimeOffset StartTime { get; set; }

        [Column("end_time")]
        public DateTimeOffset EndTime { get; set; }

        [Column("price_hour")]
        public decimal? PriceHour { get; set; }

        [Column("total_price")]
        public decimal? TotalPrice { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("trip_started_at")]
        public DateTimeOffset? TripStartedAt { get; set; }

        [Column("trip_completed_at")]
        public DateTimeOffset? TripCompletedAt { get; set; }

        [Column("completion_review_ticket_id")]
        public Guid? CompletionReviewTicketId { get; set; }

        [Column("partner_cancellation_ticket_id")]
        public Guid? PartnerCancellationTicketId { get; set; }

        [Column("partner_cancellation_requested_at")]
        public DateTimeOffset? PartnerCancellationRequestedAt { get; set; }

        [Column("cancellation_actor")]
        public string? CancellationActor { get; set; }

        [Column("cancellation_reason")]
        public string? CancellationReason { get; set; }

        [Column("car_comment_id")]
        public int? CarCommentId { get; set; }

        [Column("car_comment_submitted_at")]
        public DateTimeOffset? CarCommentSubmittedAt { get; set; }

        [Column("pricing_breakdown")]
        public string? PricingBreakdownJson { get; private set; }

        [Column("image_urls")]
        public string? ImageUrlsJson { get; private set; }

        [NotMapped]
        public BookingPricingBreakdownSnapshot? PricingBreakdown
        {
            get
            {
                if (_pricingBreakdown is not null)
                {
                    return _pricingBreakdown;
                }

                if (string.IsNullOrWhiteSpace(PricingBreakdownJson))
                {
                    return null;
                }

                try
                {
                    _pricingBreakdown = JsonSerializer.Deserialize<BookingPricingBreakdownSnapshot>(
                        PricingBreakdownJson,
                        PricingBreakdownSerializerOptions);
                }
                catch (JsonException)
                {
                    _pricingBreakdown = null;
                }

                return _pricingBreakdown;
            }
            set
            {
                _pricingBreakdown = value;
                PricingBreakdownJson = value is null
                    ? null
                    : JsonSerializer.Serialize(value, PricingBreakdownSerializerOptions);
            }
        }

        [NotMapped]
        public IReadOnlyList<string> ImageUrls
        {
            get
            {
                if (_imageUrls is not null)
                {
                    return _imageUrls;
                }

                if (string.IsNullOrWhiteSpace(ImageUrlsJson))
                {
                    _imageUrls = [];
                    return _imageUrls;
                }

                try
                {
                    _imageUrls = JsonSerializer.Deserialize<List<string>>(
                        ImageUrlsJson,
                        PricingBreakdownSerializerOptions) ?? [];
                }
                catch (JsonException)
                {
                    _imageUrls = [];
                }

                return _imageUrls;
            }
            set
            {
                _imageUrls = value?
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .ToList() ?? [];

                ImageUrlsJson = _imageUrls.Count == 0
                    ? null
                    : JsonSerializer.Serialize(_imageUrls, PricingBreakdownSerializerOptions);
            }
        }

        [Column("status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
