using CarService.Application.DTOs.CarImage;
using CarService.Application.Interfaces;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public class CarImageService : ICarImageService
    {
        private readonly ApplicationDbContext _db;

        public CarImageService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CarImageCreateResponseDto> Create(int carId, CarImageCreateRequestDto dto)
        {
            var carImage = new CarImage
            {
                CarId = carId,
                ImageType = dto.ImageType,
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder
            };

            _db.CarImages.Add(carImage);
            await _db.SaveChangesAsync();

            return new CarImageCreateResponseDto
            {
                Id = carImage.Id,
                ImageType = carImage.ImageType,
                ImageUrl = carImage.ImageUrl,
                DisplayOrder = carImage.DisplayOrder,
                CarId = carImage.CarId
            };
        }

        public async Task<IEnumerable<CarImageDto>> GetByCarId(int carId)
        {
            var images = await _db.CarImages
                .Where(ci => ci.CarId == carId)
                .OrderBy(ci => ci.DisplayOrder)
                .Select(ci => new CarImageDto
                {
                    Id = ci.Id,
                    ImageUrl = ci.ImageUrl,
                    ImageType = ci.ImageType,
                    DisplayOrder = ci.DisplayOrder
                })
                .ToListAsync();

            return images;
        }
    }
}
