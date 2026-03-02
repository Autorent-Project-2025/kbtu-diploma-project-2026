using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class Feature
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        public List<CarFeature> CarFeatures { get; set; } = [];
    }
}
