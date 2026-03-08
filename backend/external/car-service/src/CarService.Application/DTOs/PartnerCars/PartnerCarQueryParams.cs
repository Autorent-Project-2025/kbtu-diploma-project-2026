using CarService.Application.DTOs.Common;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarQueryParams : PaginationParams
    {
        public int? CarModelId { get; set; }
        public PartnerCarStatus? Status { get; set; }
        public Guid? PartnerUserId { get; set; }
    }
}
