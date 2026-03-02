using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class CarFeature
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("car_id")]
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        [Column("feature_id")]
        public int FeatureId { get; set; }
        public Feature Feature { get; set; } = null!;
    }
}
