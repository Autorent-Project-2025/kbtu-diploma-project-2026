using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class Car
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("brand")]
        public string Brand { get; set; } = null!;

        [Column("model")]
        public string Model { get; set; } = null!;

        [Column("year")]
        public int Year { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("price_hour")]
        public decimal? PriceHour { get; set; }

        [Column("price_day")]
        public decimal? PriceDay { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("engine")]
        public string? Engine { get; set; }

        [Column("transmission")]
        public string? Transmission { get; set; }

        [Column("seats")]
        public int? Seats { get; set; }

        [Column("fuel_type")]
        public string? FuelType { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("doors")]
        public int? Doors { get; set; }

        public List<CarComment> Comments { get; set; } = new();
        public List<CarImage> CarImages { get; set; } = new();
        public List<CarFeature> CarFeatures { get; set; } = new();
    }
}
