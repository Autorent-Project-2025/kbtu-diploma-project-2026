using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities
{
    public class SubscriptionPlan
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("plan_type")]
        public string PlanType { get; set; } = string.Empty; // weekly / monthly

        [Column("price")]
        public decimal Price { get; set; }

        [Column("included_bookings")]
        public int IncludedBookings { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        public List<Subscription> Subscriptions { get; set; } = [];
    }
}