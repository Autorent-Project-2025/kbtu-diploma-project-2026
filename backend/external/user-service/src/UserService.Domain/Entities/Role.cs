using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities
{
    public class Role
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
