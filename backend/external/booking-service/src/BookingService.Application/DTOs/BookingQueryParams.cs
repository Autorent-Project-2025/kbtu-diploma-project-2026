using BookingService.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Booking
{
    public class BookingQueryParams : PaginationParams
    {
        public string? SortBy { get; set; }

        [RegularExpression("^(?i:asc|desc)$", ErrorMessage = "SortOrder must be 'asc' or 'desc'.")]
        public string? SortOrder { get; set; } = "asc";
    }
}
