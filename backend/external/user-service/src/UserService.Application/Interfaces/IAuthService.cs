namespace UserService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(string Message, int UserId, string RoleName)> Register(string name, string email, string password, string? roleName = null);

        Task<string> Login(string email, string password);
    }
}
