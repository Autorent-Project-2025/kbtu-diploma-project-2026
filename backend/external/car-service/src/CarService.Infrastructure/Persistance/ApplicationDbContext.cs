using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Persistance
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Car> CarModels => Set<Car>();
        public DbSet<PartnerCar> PartnerCars => Set<PartnerCar>();
        public DbSet<CarComment> CarComments => Set<CarComment>();
        public DbSet<CarModelImage> CarModelImages => Set<CarModelImage>();
        public DbSet<PartnerCarImage> PartnerCarImages => Set<PartnerCarImage>();
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
