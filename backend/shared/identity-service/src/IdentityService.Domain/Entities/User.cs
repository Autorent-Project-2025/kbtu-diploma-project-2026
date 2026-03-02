namespace IdentityService.Domain.Entities
{
    public class User
    {
        private readonly List<Role> _roles = new();

        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

        private User() { }

        public User(Guid id, string username, string email, string passwordHash)
        {
            Id = id;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }

        public void ChangeEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("Email cannot be empty");

            Email = newEmail;
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }

        public void AddRole(Role role)
        {
            if (_roles.Any(r => r.Id == role.Id))
                return;

            _roles.Add(role);
        }

        public void RemoveRole(Guid roleId)
        {
            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role != null)
                _roles.Remove(role);
        }

        public bool HasPermission(string permissionName)
        {
            return _roles.Any(r => r.HasPermission(permissionName));
        }
    }
}