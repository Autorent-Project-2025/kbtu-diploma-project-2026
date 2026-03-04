using CarService.Application.DTOs.CarImage;

namespace CarService.Application.Interfaces
{
    public interface ICarImageService
    {
        Task<CarImageCreateResponseDto> CreateModelImageAsync(
            int modelId,
            CarImageCreateRequestDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default);

        Task<CarImageCreateResponseDto> CreatePartnerCarImageAsync(
            Guid currentUserId,
            int partnerCarId,
            CarImageCreateRequestDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CarImageDto>> GetModelImagesAsync(int modelId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarImageDto>> GetPartnerCarImagesAsync(int partnerCarId, CancellationToken cancellationToken = default);

        Task<CarImageCreateResponseDto?> UpdateModelImageAsync(
            int imageId,
            CarImageUpdateDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default);

        Task<CarImageCreateResponseDto?> UpdatePartnerCarImageAsync(
            Guid currentUserId,
            int imageId,
            CarImageUpdateDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteModelImageAsync(int imageId, string authorizationHeader, CancellationToken cancellationToken = default);
        Task<bool> DeletePartnerCarImageAsync(Guid currentUserId, int imageId, string authorizationHeader, CancellationToken cancellationToken = default);
    }
}
