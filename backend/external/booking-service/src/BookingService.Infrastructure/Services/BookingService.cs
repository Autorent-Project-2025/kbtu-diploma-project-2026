using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;
using BookingService.Application.Interfaces;
using BookingService.Application.Mappers;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace BookingService.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private const int MaxSerializableRetries = 3;
        private static readonly SemaphoreSlim InMemoryCreateLock = new(1, 1);

        private readonly ApplicationDbContext _db;

        public BookingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsCarAvailable(int carId, DateTime start, DateTime end)
        {
            if (carId <= 0)
            {
                throw new ArgumentException("CarId must be greater than zero.", nameof(carId));
            }

            EnsureValidDateRange(start, end);

            return !await HasOverlappingActiveBookings(carId, start, end);
        }

        public async Task<BookingResponseDto> CreateBooking(string userId, BookingCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            EnsureValidUserId(userId);

            if (dto.CarId <= 0)
            {
                throw new ArgumentException("CarId must be greater than zero.", nameof(dto.CarId));
            }

            EnsureValidDateRange(dto.StartDate, dto.EndDate);

            if (!_db.Database.IsRelational())
            {
                return await CreateBookingInMemory(userId, dto);
            }

            for (var attempt = 1; attempt <= MaxSerializableRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    var booking = await CreateBookingWithOverlapCheck(userId, dto);
                    await transaction.CommitAsync();
                    return booking.ToBookingResponseDto();
                }
                catch (PostgresException ex) when (IsSerializationFailure(ex))
                {
                    await transaction.RollbackAsync();
                    _db.ChangeTracker.Clear();

                    if (attempt == MaxSerializableRetries)
                    {
                        throw new InvalidOperationException("Car is already booked for this time.");
                    }
                }
                catch (DbUpdateException ex) when (IsSerializationFailure(ex))
                {
                    await transaction.RollbackAsync();
                    _db.ChangeTracker.Clear();

                    if (attempt == MaxSerializableRetries)
                    {
                        throw new InvalidOperationException("Car is already booked for this time.");
                    }
                }
            }

            throw new InvalidOperationException("Car is already booked for this time.");
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookings(string userId)
        {
            EnsureValidUserId(userId);

            return await _db.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .SelectToBookingResponseDto()
                .ToListAsync();
        }

        public async Task<PagedResult<BookingResponseDto>> GetUserBookingsPaginated(string userId, BookingQueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            EnsureValidUserId(userId);

            var sortBy = NormalizeSortBy(queryParams.SortBy);
            var isDescending = string.Equals(queryParams.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<Booking> query = _db.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId);

            query = sortBy switch
            {
                "startdate" => isDescending ? query.OrderByDescending(b => b.StartDate) : query.OrderBy(b => b.StartDate),
                "enddate" => isDescending ? query.OrderByDescending(b => b.EndDate) : query.OrderBy(b => b.EndDate),
                _ => isDescending ? query.OrderByDescending(b => b.Id) : query.OrderBy(b => b.Id)
            };

            var totalCount = await query.CountAsync();

            var bookings = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .SelectToBookingResponseDto()
                .ToListAsync();

            return new PagedResult<BookingResponseDto>
            {
                Items = bookings,
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<BookingResponseDto?> GetBooking(int id, string userId)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            EnsureValidUserId(userId);

            return await _db.Bookings
                .AsNoTracking()
                .Where(b => b.Id == id && b.UserId == userId)
                .SelectToBookingResponseDto()
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByCarId(int carId, CancellationToken cancellationToken = default)
        {
            if (carId <= 0)
            {
                throw new ArgumentException("CarId must be greater than zero.", nameof(carId));
            }

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => booking.CarId == carId)
                .OrderByDescending(booking => booking.StartDate)
                .ThenByDescending(booking => booking.Id)
                .SelectToBookingResponseDto()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByCarIds(IReadOnlyCollection<int> carIds, CancellationToken cancellationToken = default)
        {
            var normalizedIds = carIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (normalizedIds.Length == 0)
            {
                return [];
            }

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => normalizedIds.Contains(booking.CarId))
                .GroupBy(booking => booking.CarId)
                .Select(group => new CarBookingCountDto
                {
                    CarId = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CancelBooking(int id, string userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            ApplyStatusTransition(booking, BookingStatus.Canceled);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmBooking(int id, string userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            ApplyStatusTransition(booking, BookingStatus.Confirmed);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteBooking(int id, string userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            ApplyStatusTransition(booking, BookingStatus.Completed);
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<BookingResponseDto> CreateBookingInMemory(string userId, BookingCreateDto dto)
        {
            await InMemoryCreateLock.WaitAsync();
            try
            {
                var booking = await CreateBookingWithOverlapCheck(userId, dto);
                return booking.ToBookingResponseDto();
            }
            finally
            {
                InMemoryCreateLock.Release();
            }
        }

        private async Task<Booking> CreateBookingWithOverlapCheck(string userId, BookingCreateDto dto)
        {
            if (await HasOverlappingActiveBookings(dto.CarId, dto.StartDate, dto.EndDate))
            {
                throw new InvalidOperationException("Car is already booked for this time.");
            }

            var hours = (decimal)(dto.EndDate - dto.StartDate).TotalHours;

            var booking = new Booking
            {
                CarId = dto.CarId,
                UserId = userId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = BookingStatus.Pending,
                Price = hours
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        private Task<bool> HasOverlappingActiveBookings(int carId, DateTime start, DateTime end)
        {
            return _db.Bookings
                .AnyAsync(b =>
                    b.CarId == carId &&
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                    start < b.EndDate &&
                    end > b.StartDate);
        }

        private async Task<Booking?> GetUserBookingEntity(int id, string userId)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            EnsureValidUserId(userId);

            return await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        }

        private static void EnsureValidDateRange(DateTime start, DateTime end)
        {
            if (start == default)
            {
                throw new ArgumentException("StartDate is required.", nameof(start));
            }

            if (end == default)
            {
                throw new ArgumentException("EndDate is required.", nameof(end));
            }

            if (end <= start)
            {
                throw new ArgumentException("EndDate must be greater than StartDate.", nameof(end));
            }
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "id";
            }

            var normalized = sortBy.Trim().ToLowerInvariant();
            if (normalized is "id" or "startdate" or "enddate")
            {
                return normalized;
            }

            throw new ArgumentException("SortBy must be one of: id, startDate, endDate.", nameof(sortBy));
        }

        private static void EnsureValidUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }
        }

        private static void ApplyStatusTransition(Booking booking, BookingStatus targetStatus)
        {
            if (booking.Status == targetStatus)
            {
                throw new InvalidOperationException($"Booking is already {targetStatus}.");
            }

            var isAllowed = booking.Status switch
            {
                BookingStatus.Pending => targetStatus is BookingStatus.Confirmed or BookingStatus.Canceled,
                BookingStatus.Confirmed => targetStatus is BookingStatus.Completed or BookingStatus.Canceled,
                BookingStatus.Completed => false,
                BookingStatus.Canceled => false,
                _ => false
            };

            if (!isAllowed)
            {
                throw new InvalidOperationException(
                    $"Cannot change booking status from {booking.Status} to {targetStatus}.");
            }

            booking.Status = targetStatus;
        }

        private static bool IsSerializationFailure(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgresException && IsSerializationFailure(postgresException);
        }

        private static bool IsSerializationFailure(PostgresException ex)
        {
            return ex.SqlState == PostgresErrorCodes.SerializationFailure;
        }
    }
}
