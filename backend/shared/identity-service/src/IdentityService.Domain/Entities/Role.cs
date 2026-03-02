namespace IdentityService.Domain.Entities
{
    public class Role
    {
        private readonly List<Permission> _permissions = new();

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public IReadOnlyCollection<Permission> Permissions => _permissions;

        private Role() { }

        public Role(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddPermission(Permission permission)
        {
            if (_permissions.Any(p => p.Id == permission.Id))
                return;

            _permissions.Add(permission);
        }

        public bool HasPermission(string permissionName)
        {
            return _permissions.Any(p => p.Name == permissionName);
        }
    }
}
