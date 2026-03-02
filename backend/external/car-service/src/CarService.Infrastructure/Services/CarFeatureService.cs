using CarService.Application.DTOs.CarFeature;
using CarService.Application.Interfaces;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public class CarFeatureService : ICarFeatureService
    {
        private readonly ApplicationDbContext _db;

        public CarFeatureService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CarFeatureDto>> GetAllAsync()
        {
            return await _db.Features
                .Select(f => new CarFeatureDto
                {
                    Name = f.Name
                })
                .ToListAsync();
        }
    }
}
