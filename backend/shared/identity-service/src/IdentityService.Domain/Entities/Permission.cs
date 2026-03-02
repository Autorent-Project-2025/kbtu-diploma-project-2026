namespace IdentityService.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        private Permission() { }

        public Permission(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
