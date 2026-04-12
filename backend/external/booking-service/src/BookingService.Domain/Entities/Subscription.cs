using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities
{
    public class Subscription
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("subscription_plan_id")]
        public int SubscriptionPlanId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active"; // active / cancelled / expired

        [Column("start_date")]
        public DateTimeOffset StartDate { get; set; }

        [Column("end_date")]
        public DateTimeOffset EndDate { get; set; }

        [Column("auto_renew")]
        public bool AutoRenew { get; set; }

        [Column("included_bookings")]
        public int IncludedBookings { get; set; }

        [Column("used_bookings")]
        public int UsedBookings { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        public SubscriptionPlan Plan { get; set; } = null!;
    }
}