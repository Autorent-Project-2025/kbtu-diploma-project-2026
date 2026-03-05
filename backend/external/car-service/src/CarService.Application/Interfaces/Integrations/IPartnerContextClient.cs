namespace CarService.Application.Interfaces.Integrations
{
    public interface IPartnerContextClient
    {
        Task<PartnerContextDto?> GetCurrentPartnerAsync(string authorizationHeader, CancellationToken cancellationToken = default);
    }

    public sealed class PartnerContextDto
    {
        public int Id { get; set; }
        public string RelatedUserId { get; set; } = string.Empty;
        public string OwnerFirstName { get; set; } = string.Empty;
        public string OwnerLastName { get; set; } = string.Empty;
    }
}
