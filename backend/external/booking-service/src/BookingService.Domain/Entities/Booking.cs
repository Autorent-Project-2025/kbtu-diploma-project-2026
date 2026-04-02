using BookingService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities
{
    public class Booking
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("partner_car_id")]
        public int PartnerCarId { get; set; }

        [Column("partner_user_id")]
        public Guid PartnerUserId { get; set; }

        [Column("start_time")]
        public DateTimeOffset StartTime { get; set; }

        [Column("end_time")]
        public DateTimeOffset EndTime { get; set; }

        [Column("price_hour")]
        public decimal? PriceHour { get; set; }

        [Column("total_price")]
        public decimal? TotalPrice { get; set; }

        [Column("subscription_id")]
        public int? SubscriptionId { get; set; }

        [Column("used_subscription")]
        public bool UsedSubscription { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("trip_started_at")]
        public DateTimeOffset? TripStartedAt { get; set; }

        [Column("trip_completed_at")]
        public DateTimeOffset? TripCompletedAt { get; set; }

        [Column("completion_review_ticket_id")]
        public Guid? CompletionReviewTicketId { get; set; }

        [Column("status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}