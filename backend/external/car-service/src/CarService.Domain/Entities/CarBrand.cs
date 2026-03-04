using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class CarBrand
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        public List<CarModelLookup> Models { get; set; } = [];
        public List<Car> CarModels { get; set; } = [];
    }
}
