using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Services
{
    public class EfRoleProvider : IRoleProvider
    {
        private readonly ApplicationDbContext _db;

        public EfRoleProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetRoleIdAsync(string roleName)
        {
            var roleId = await _db.Roles
                .Where(x => x.Name == roleName)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (roleId <= 0)
                throw new InvalidOperationException($"Role '{roleName}' not found.");

            return roleId;
        }
    }
}
