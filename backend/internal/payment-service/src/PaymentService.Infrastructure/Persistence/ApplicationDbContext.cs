using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PartnerWallet> PartnerWallets => Set<PartnerWallet>();
    public DbSet<PartnerLedgerEntry> PartnerLedgerEntries => Set<PartnerLedgerEntry>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
    public DbSet<PartnerPayout> PartnerPayouts => Set<PartnerPayout>();
    public DbSet<MockPaymentAttempt> MockPaymentAttempts => Set<MockPaymentAttempt>();
    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
