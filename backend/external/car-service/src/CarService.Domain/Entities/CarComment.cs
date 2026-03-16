using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class CarComment
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("user_name")]
        public string UserName { get; set; } = string.Empty;

        [Column("car_id")]
        public int CarId { get; set; }

        [Column("partner_car_id")]
        public int? PartnerCarId { get; set; }

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("rating")]
        public int Rating { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        public Car Car { get; set; } = null!;
        public PartnerCar? PartnerCar { get; set; }
    }
}
