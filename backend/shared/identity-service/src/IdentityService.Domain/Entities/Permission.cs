namespace IdentityService.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public ICollection<Role> Roles { get; private set; } = new List<Role>();

    private Permission() { }

    public Permission(Guid id, string name, string description)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetName(name);
        SetDescription(description);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Permission name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    public void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }
}
