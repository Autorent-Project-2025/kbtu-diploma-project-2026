using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    internal static class CarCatalogQueryExtensions
    {
        public static IQueryable<Car> IncludeCatalog(this IQueryable<Car> query)
        {
            return query
                .Include(entity => entity.Brand)
                .Include(entity => entity.ModelLookup);
        }

        public static IQueryable<PartnerCar> IncludeModelCatalog(this IQueryable<PartnerCar> query)
        {
            return query
                .Include(entity => entity.CarModel)
                    .ThenInclude(model => model.Brand)
                .Include(entity => entity.CarModel)
                    .ThenInclude(model => model.ModelLookup);
        }
    }
}
