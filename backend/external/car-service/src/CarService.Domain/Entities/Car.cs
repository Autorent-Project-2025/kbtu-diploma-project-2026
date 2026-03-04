using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class Car
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("brand_id")]
        public int BrandId { get; set; }

        [Column("model_id")]
        public int ModelId { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("engine")]
        public string? Engine { get; set; }

        [Column("transmission")]
        public string? Transmission { get; set; }

        [Column("seats")]
        public int? Seats { get; set; }

        [Column("fuel_type")]
        public string? FuelType { get; set; }

        [Column("doors")]
        public int? Doors { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("ratings_count")]
        public int RatingsCount { get; set; }

        public CarBrand Brand { get; set; } = null!;
        public CarModelLookup ModelLookup { get; set; } = null!;
        public List<CarFeature> CarFeatures { get; set; } = [];
        public List<PartnerCar> PartnerCars { get; set; } = [];
        public List<CarModelImage> ModelImages { get; set; } = [];
        public List<CarComment> Comments { get; set; } = [];
    }
}
