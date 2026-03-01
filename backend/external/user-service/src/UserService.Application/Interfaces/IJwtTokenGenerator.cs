using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        public string GenerateToken(User user);
    }
}
