using CarService.Application.DTOs.Common;

namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentQueryParams : PaginationParams
    {
        public int PartnerCarId { get; set; }
    }
}
