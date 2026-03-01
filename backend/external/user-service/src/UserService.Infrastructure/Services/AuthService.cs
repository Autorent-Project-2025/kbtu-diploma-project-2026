using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IRoleProvider _roleProvider;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(ApplicationDbContext db, IConfiguration config, IRoleProvider roleProvider, IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleProvider = roleProvider;
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                throw new InvalidCredentialsException("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new InvalidCredentialsException("Invalid email or password");

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task<(string Message, int UserId, string RoleName)> Register(string name, string email, string password, string? roleName = null)
        {
            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new UserAlreadyExistsException("User already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var normalizedRoleName = string.IsNullOrWhiteSpace(roleName)
                ? RoleConstants.User
                : roleName.Trim().ToLowerInvariant();

            var userRoleId = await _roleProvider.GetRoleIdAsync(normalizedRoleName);

            var user = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                RoleId = userRoleId
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return ("User created", user.Id, normalizedRoleName);
        }
    }
}
