using Microsoft.EntityFrameworkCore;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence;

public sealed class TicketDbContext : DbContext, ITicketUnitOfWork
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
