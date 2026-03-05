using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class CarModelLookup
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("brand_id")]
        public int BrandId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        public CarBrand Brand { get; set; } = null!;
        public List<Car> CarModels { get; set; } = [];
    }
}
