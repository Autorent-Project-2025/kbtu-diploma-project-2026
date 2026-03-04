using CarService.Application.DTOs.CarImage;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class CarImageService : ICarImageService
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageStorageClient _imageStorageClient;

        public CarImageService(ApplicationDbContext db, IImageStorageClient imageStorageClient)
        {
            _db = db;
            _imageStorageClient = imageStorageClient;
        }

        public async Task<CarImageCreateResponseDto> CreateModelImageAsync(
            int modelId,
            CarImageCreateRequestDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            if (!await _db.CarModels.AnyAsync(model => model.Id == modelId, cancellationToken))
            {
                throw new KeyNotFoundException($"Car model with id {modelId} was not found.");
            }

            var upload = await _imageStorageClient.UploadAsync(
                DecodeBase64(dto.Base64Content),
                authorizationHeader,
                cancellationToken);

            var entity = new CarModelImage
            {
                ModelId = modelId,
                ImageId = upload.ImageId,
                ImageUrl = upload.ImageUrl,
                ImageType = dto.ImageType,
                DisplayOrder = dto.DisplayOrder
            };

            _db.CarModelImages.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<CarImageCreateResponseDto> CreatePartnerCarImageAsync(
            Guid currentUserId,
            int partnerCarId,
            CarImageCreateRequestDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            var partnerCar = await _db.PartnerCars
                .FirstOrDefaultAsync(car => car.Id == partnerCarId, cancellationToken);

            if (partnerCar is null)
            {
                throw new KeyNotFoundException($"Partner car with id {partnerCarId} was not found.");
            }

            if (partnerCar.PartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to add images for this partner car.");
            }

            var upload = await _imageStorageClient.UploadAsync(
                DecodeBase64(dto.Base64Content),
                authorizationHeader,
                cancellationToken);

            var entity = new PartnerCarImage
            {
                CarId = partnerCarId,
                ImageId = upload.ImageId,
                ImageUrl = upload.ImageUrl,
                ImageType = dto.ImageType,
                DisplayOrder = dto.DisplayOrder
            };

            _db.PartnerCarImages.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<IReadOnlyCollection<CarImageDto>> GetModelImagesAsync(int modelId, CancellationToken cancellationToken = default)
        {
            return await _db.CarModelImages
                .AsNoTracking()
                .Where(image => image.ModelId == modelId)
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(image => new CarImageDto
                {
                    Id = image.Id,
                    ImageId = image.ImageId,
                    ImageUrl = image.ImageUrl,
                    ImageType = image.ImageType,
                    DisplayOrder = image.DisplayOrder
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<CarImageDto>> GetPartnerCarImagesAsync(int partnerCarId, CancellationToken cancellationToken = default)
        {
            return await _db.PartnerCarImages
                .AsNoTracking()
                .Where(image => image.CarId == partnerCarId)
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(image => new CarImageDto
                {
                    Id = image.Id,
                    ImageId = image.ImageId,
                    ImageUrl = image.ImageUrl,
                    ImageType = image.ImageType,
                    DisplayOrder = image.DisplayOrder
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<CarImageCreateResponseDto?> UpdateModelImageAsync(
            int imageId,
            CarImageUpdateDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarModelImages.FirstOrDefaultAsync(image => image.Id == imageId, cancellationToken);
            if (entity is null)
            {
                return null;
            }

            await ReplaceBinaryIfNeededAsync(entity, dto.Base64Content, authorizationHeader, cancellationToken);

            entity.ImageType = dto.ImageType;
            entity.DisplayOrder = dto.DisplayOrder;

            await _db.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<CarImageCreateResponseDto?> UpdatePartnerCarImageAsync(
            Guid currentUserId,
            int imageId,
            CarImageUpdateDto dto,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCarImages
                .Include(image => image.Car)
                .FirstOrDefaultAsync(image => image.Id == imageId, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            if (entity.Car.PartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to update this partner car image.");
            }

            await ReplaceBinaryIfNeededAsync(entity, dto.Base64Content, authorizationHeader, cancellationToken);

            entity.ImageType = dto.ImageType;
            entity.DisplayOrder = dto.DisplayOrder;

            await _db.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<bool> DeleteModelImageAsync(
            int imageId,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarModelImages.FirstOrDefaultAsync(image => image.Id == imageId, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(entity.ImageId))
            {
                await _imageStorageClient.DeleteAsync(entity.ImageId, authorizationHeader, cancellationToken);
            }

            _db.CarModelImages.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePartnerCarImageAsync(
            Guid currentUserId,
            int imageId,
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCarImages
                .Include(image => image.Car)
                .FirstOrDefaultAsync(image => image.Id == imageId, cancellationToken);

            if (entity is null)
            {
                return false;
            }

            if (entity.Car.PartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this partner car image.");
            }

            if (!string.IsNullOrWhiteSpace(entity.ImageId))
            {
                await _imageStorageClient.DeleteAsync(entity.ImageId, authorizationHeader, cancellationToken);
            }

            _db.PartnerCarImages.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task ReplaceBinaryIfNeededAsync(
            CarModelImage entity,
            string? base64Content,
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(base64Content))
            {
                return;
            }

            var upload = await _imageStorageClient.UploadAsync(
                DecodeBase64(base64Content),
                authorizationHeader,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(entity.ImageId))
            {
                await _imageStorageClient.DeleteAsync(entity.ImageId, authorizationHeader, cancellationToken);
            }

            entity.ImageId = upload.ImageId;
            entity.ImageUrl = upload.ImageUrl;
        }

        private async Task ReplaceBinaryIfNeededAsync(
            PartnerCarImage entity,
            string? base64Content,
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(base64Content))
            {
                return;
            }

            var upload = await _imageStorageClient.UploadAsync(
                DecodeBase64(base64Content),
                authorizationHeader,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(entity.ImageId))
            {
                await _imageStorageClient.DeleteAsync(entity.ImageId, authorizationHeader, cancellationToken);
            }

            entity.ImageId = upload.ImageId;
            entity.ImageUrl = upload.ImageUrl;
        }

        private static byte[] DecodeBase64(string base64Content)
        {
            if (string.IsNullOrWhiteSpace(base64Content))
            {
                throw new ArgumentException("Base64Content is required.", nameof(base64Content));
            }

            var payload = base64Content.Trim();
            var marker = payload.IndexOf(",", StringComparison.Ordinal);
            if (marker >= 0)
            {
                payload = payload[(marker + 1)..];
            }

            try
            {
                return Convert.FromBase64String(payload);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Base64Content is invalid.", nameof(base64Content));
            }
        }

        private static CarImageCreateResponseDto MapToResponse(CarModelImage entity)
        {
            return new CarImageCreateResponseDto
            {
                Id = entity.Id,
                ModelId = entity.ModelId,
                PartnerCarId = null,
                ImageId = entity.ImageId,
                ImageUrl = entity.ImageUrl,
                ImageType = entity.ImageType,
                DisplayOrder = entity.DisplayOrder
            };
        }

        private static CarImageCreateResponseDto MapToResponse(PartnerCarImage entity)
        {
            return new CarImageCreateResponseDto
            {
                Id = entity.Id,
                ModelId = null,
                PartnerCarId = entity.CarId,
                ImageId = entity.ImageId,
                ImageUrl = entity.ImageUrl,
                ImageType = entity.ImageType,
                DisplayOrder = entity.DisplayOrder
            };
        }
    }
}
