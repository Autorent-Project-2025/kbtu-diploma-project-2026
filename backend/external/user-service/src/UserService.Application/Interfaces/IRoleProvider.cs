namespace UserService.Application.Interfaces
{
    public interface IRoleProvider
    {
        public Task<int> GetRoleIdAsync(string roleName);
    }
}
