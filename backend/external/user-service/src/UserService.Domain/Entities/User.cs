using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities
{
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("role_id")]
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
