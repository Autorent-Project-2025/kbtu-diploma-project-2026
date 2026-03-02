using CarService.Application.DTOs.Common;
using CarService.Application.Enums;

namespace CarService.Application.DTOs.Cars
{
    public class CarQueryParams : PaginationParams
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public CarSortOption SortBy { get; set; } = CarSortOption.None;
        public string? SortOrder { get; set; } = "asc";
    }
}
