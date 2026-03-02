using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Persistance
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Car> Cars => Set<Car>();
        public DbSet<CarComment> CarComments => Set<CarComment>();
        public DbSet<CarImage> CarImages => Set<CarImage>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<CarFeature> CarFeatures => Set<CarFeature>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
