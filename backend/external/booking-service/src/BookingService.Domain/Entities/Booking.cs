using BookingService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities
{
    public class Booking
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("car_id")]
        public int CarId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
