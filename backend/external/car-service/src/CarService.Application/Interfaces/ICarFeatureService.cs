using CarService.Application.DTOs.CarFeature;

namespace CarService.Application.Interfaces
{
    public interface ICarFeatureService
    {
        Task<IEnumerable<CarFeatureDto>> GetAllAsync();
    }
}
