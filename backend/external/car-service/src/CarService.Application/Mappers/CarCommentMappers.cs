using CarService.Application.DTOs.CarComment;
using CarService.Domain.Entities;
using System.Linq.Expressions;

namespace CarService.Application.Mappers
{
    public static class CarCommentMappers
    {
        private static readonly Expression<Func<CarComment, CarCommentDetailsResponseDto>> CarCommentDetailsProjection = comment => new CarCommentDetailsResponseDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            CarId = comment.CarId,
            Content = comment.Content,
            Rating = comment.Rating,
            Created_On = comment.CreatedOn,
            Username = comment.UserName
        };

        public static IQueryable<CarCommentDetailsResponseDto> SelectToCarCommentDetailsResponseDto(this IQueryable<CarComment> query)
        {
            return query.Select(CarCommentDetailsProjection);
        }

        public static CarCommentResponseDto ToCarCommentResponseDto(this CarComment comment)
        {
            return new CarCommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                CarId = comment.CarId,
                Content = comment.Content,
                Rating = comment.Rating,
                Created_On = comment.CreatedOn
            };
        }

        public static CarComment ToCarCommentEntity(this CarCommentCreateDto dto, int userId, DateTime createdOn)
        {
            return new CarComment
            {
                UserId = userId,
                UserName = $"user-{userId}",
                CarId = dto.CarId,
                Content = dto.Content,
                Rating = dto.Rating,
                CreatedOn = createdOn
            };
        }

        public static void ApplyUpdate(this CarComment comment, CarCommentUpdateDto dto)
        {
            comment.Content = dto.Content;
            comment.Rating = dto.Rating;
        }
    }
}
