using CarService.Application.DTOs.CarFeature;
using CarService.Application.DTOs.Cars;
using CarService.Domain.Entities;
using System.Linq.Expressions;

namespace CarService.Application.Mappers
{
    public static class CarMappers
    {
        private static readonly Expression<Func<Car, CarResponseDto>> CarResponseProjection = car => new CarResponseDto
        {
            Id = car.Id,
            Brand = car.Brand,
            Model = car.Model,
            Year = car.Year,
            PriceHour = car.PriceHour,
            PriceDay = car.PriceDay,
            ImageUrl = car.ImageUrl,
            Rating = car.Rating
        };

        private static readonly Expression<Func<Car, CarResponseDto>> CarResponseWithDescriptionProjection = car => new CarResponseDto
        {
            Id = car.Id,
            Brand = car.Brand,
            Model = car.Model,
            Year = car.Year,
            PriceHour = car.PriceHour,
            PriceDay = car.PriceDay,
            ImageUrl = car.ImageUrl,
            Rating = car.Rating
        };

        public static IQueryable<CarResponseDto> SelectToCarResponseDto(this IQueryable<Car> query)
        {
            return query.Select(CarResponseProjection);
        }

        public static IQueryable<CarResponseDto> SelectToCarResponseDtoWithDescription(this IQueryable<Car> query)
        {
            return query.Select(CarResponseWithDescriptionProjection);
        }

        public static CarResponseDto ToCarResponseDto(this Car car)
        {
            return new CarResponseDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PriceHour = car.PriceHour,
                PriceDay = car.PriceDay,
                ImageUrl = car.ImageUrl,
                Rating = car.Rating
            };
        }

        public static CarDetailsResponseDto ToCarDetailsResponseDto(this Car car)
        {
            return new CarDetailsResponseDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Rating = car.Rating,
                Model = car.Model,
                Year = car.Year,
                PriceHour = car.PriceHour,
                PriceDay = car.PriceDay,
                ImageUrl = car.ImageUrl,
                Description = car.Description,
                Engine = car.Engine,
                Doors = car.Doors,
                Transmission = car.Transmission,
                Seats = car.Seats,
                FuelType = car.FuelType,
                Color = car.Color,
                Comments = car.Comments
                    .Select(comment => new CarDetailsCommentDto
                    {
                        Id = comment.Id,
                        Content = comment.Content,
                        Rating = comment.Rating,
                        UserId = comment.UserId,
                        Created_On = comment.CreatedOn
                    })
                    .ToList(),
                Features = car.CarFeatures
                    .Select(feature => new CarFeatureDto
                    {
                        Name = feature.Feature.Name
                    })
                    .ToList()
            };
        }

        public static CarCreateResponseDto ToCarCreateResponseDto(this Car car)
        {
            return new CarCreateResponseDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PriceHour = car.PriceHour,
                PriceDay = car.PriceDay,
                ImageUrl = car.ImageUrl,
                Rating = car.Rating,
                Description = car.Description,
                Engine = car.Engine,
                Doors = car.Doors,
                Transmission = car.Transmission,
                Seats = car.Seats,
                FuelType = car.FuelType,
                Color = car.Color,
                Features = car.CarFeatures
                    .Select(feature => new CarFeatureDto
                    {
                        Name = feature.Feature.Name
                    })
                    .ToList()
            };
        }

        public static Car ToCarEntity(this CarCreateRequestDto dto)
        {
            return new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                PriceHour = dto.PriceHour,
                PriceDay = dto.PriceDay,
                ImageUrl = dto.ImageUrl,
                Description = dto.Description
            };
        }

        public static Car ToCarEntity(this CarCreateRequestDto dto, IEnumerable<Feature> features)
        {
            return new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                PriceHour = dto.PriceHour,
                PriceDay = dto.PriceDay,
                ImageUrl = dto.ImageUrl,
                Description = dto.Description,
                Engine = dto.Engine,
                Doors = dto.Doors,
                Transmission = dto.Transmission,
                Seats = dto.Seats,
                FuelType = dto.FuelType,
                Color = dto.Color,
                CarFeatures = features.Select(feature => new CarFeature
                {
                    Feature = feature
                }).ToList()
            };
        }

        public static void ApplyUpdate(this Car car, CarUpdateDto dto)
        {
            car.Brand = dto.Brand;
            car.Model = dto.Model;
            car.Year = dto.Year;
            car.PriceHour = dto.PriceHour;
            car.PriceDay = dto.PriceDay;
            car.ImageUrl = dto.ImageUrl;
        }
    }
}