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

        [Column("market_value_kzt")]
        public decimal? MarketValueKzt { get; set; }

        [Column("market_value_fetched_at")]
        public DateTimeOffset? MarketValueFetchedAt { get; set; }

        [Column("market_value_source")]
        public string? MarketValueSource { get; set; }

        [Column("market_value_source_url")]
        public string? MarketValueSourceUrl { get; set; }

        [Column("market_value_sample_count")]
        public int MarketValueSampleCount { get; set; }

        [Column("market_value_filtered_sample_count")]
        public int MarketValueFilteredSampleCount { get; set; }

        [Column("market_value_confidence")]
        public string? MarketValueConfidence { get; set; }

        [Column("market_value_status")]
        public string? MarketValueStatus { get; set; }

        [Column("market_value_error")]
        public string? MarketValueError { get; set; }

        public CarBrand Brand { get; set; } = null!;
        public CarModelLookup ModelLookup { get; set; } = null!;
        public List<CarFeature> CarFeatures { get; set; } = [];
        public List<PartnerCar> PartnerCars { get; set; } = [];
        public List<CarModelImage> ModelImages { get; set; } = [];
        public List<CarComment> Comments { get; set; } = [];
    }
}
