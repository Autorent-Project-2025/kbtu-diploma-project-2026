using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<PaymentSyncOutboxMessage> PaymentSyncOutboxMessages => Set<PaymentSyncOutboxMessage>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}