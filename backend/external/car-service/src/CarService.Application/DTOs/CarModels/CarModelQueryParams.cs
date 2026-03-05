using CarService.Application.DTOs.Common;

namespace CarService.Application.DTOs.CarModels
{
    public class CarModelQueryParams : PaginationParams
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}
