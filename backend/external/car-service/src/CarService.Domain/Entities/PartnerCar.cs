using CarService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class PartnerCar
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("partner_id")]
        public Guid PartnerId { get; set; }

        [Column("car_model_id")]
        public int CarModelId { get; set; }

        [Column("license_plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [Column("color")]
        public string? Color { get; set; }

        [Column("price_hour")]
        public decimal? PriceHour { get; set; }

        [Column("price_day")]
        public decimal? PriceDay { get; set; }

        [Column("status")]
        public PartnerCarStatus Status { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("ratings_count")]
        public int RatingsCount { get; set; }

        public Car CarModel { get; set; } = null!;
        public List<PartnerCarImage> Images { get; set; } = [];
        public List<CarComment> Comments { get; set; } = [];
    }
}
