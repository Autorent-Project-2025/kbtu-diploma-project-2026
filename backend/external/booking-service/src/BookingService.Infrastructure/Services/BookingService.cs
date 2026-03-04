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

        public async Task<bool> IsPartnerCarAvailable(int partnerCarId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (partnerCarId <= 0)
            {
                throw new ArgumentException("PartnerCarId must be greater than zero.", nameof(partnerCarId));
            }

            EnsureValidDateRange(startTime, endTime);

            return !await HasOverlappingActiveBookings(partnerCarId, startTime, endTime);
        }

        public async Task<BookingResponseDto> CreateBooking(Guid userId, BookingCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            EnsureValidUserId(userId);

            var partnerCarId = dto.ResolvePartnerCarId();
            var startTime = dto.ResolveStartTime();
            var endTime = dto.ResolveEndTime();

            if (dto.PartnerId == Guid.Empty)
            {
                throw new ArgumentException("PartnerId is required.", nameof(dto.PartnerId));
            }

            if (dto.PriceHour.HasValue && dto.PriceHour.Value <= 0)
            {
                throw new ArgumentException("PriceHour must be greater than zero.", nameof(dto.PriceHour));
            }

            EnsureValidDateRange(startTime, endTime);

            if (!_db.Database.IsRelational())
            {
                return await CreateBookingInMemory(userId, dto, partnerCarId, startTime, endTime);
            }

            for (var attempt = 1; attempt <= MaxSerializableRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    var booking = await CreateBookingWithOverlapCheck(userId, dto, partnerCarId, startTime, endTime);
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
                catch (PostgresException ex) when (IsOverlappingConstraintViolation(ex))
                {
                    await transaction.RollbackAsync();
                    _db.ChangeTracker.Clear();
                    throw new InvalidOperationException("Car is already booked for this time.");
                }
                catch (DbUpdateException ex) when (IsOverlappingConstraintViolation(ex))
                {
                    await transaction.RollbackAsync();
                    _db.ChangeTracker.Clear();
                    throw new InvalidOperationException("Car is already booked for this time.");
                }
            }

            throw new InvalidOperationException("Car is already booked for this time.");
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookings(Guid userId)
        {
            EnsureValidUserId(userId);

            return await _db.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .SelectToBookingResponseDto()
                .ToListAsync();
        }

        public async Task<PagedResult<BookingResponseDto>> GetUserBookingsPaginated(Guid userId, BookingQueryParams queryParams)
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
                "starttime" => isDescending ? query.OrderByDescending(b => b.StartTime) : query.OrderBy(b => b.StartTime),
                "endtime" => isDescending ? query.OrderByDescending(b => b.EndTime) : query.OrderBy(b => b.EndTime),
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

        public async Task<BookingResponseDto?> GetBooking(int id, Guid userId)
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

        public async Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByPartnerCarId(int partnerCarId, CancellationToken cancellationToken = default)
        {
            if (partnerCarId <= 0)
            {
                throw new ArgumentException("PartnerCarId must be greater than zero.", nameof(partnerCarId));
            }

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => booking.PartnerCarId == partnerCarId)
                .OrderByDescending(booking => booking.StartTime)
                .ThenByDescending(booking => booking.Id)
                .SelectToBookingResponseDto()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByPartnerCarIds(IReadOnlyCollection<int> partnerCarIds, CancellationToken cancellationToken = default)
        {
            var normalizedIds = partnerCarIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (normalizedIds.Length == 0)
            {
                return [];
            }

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => normalizedIds.Contains(booking.PartnerCarId))
                .GroupBy(booking => booking.PartnerCarId)
                .Select(group => new CarBookingCountDto
                {
                    PartnerCarId = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CancelBooking(int id, Guid userId)
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

        public async Task<bool> ConfirmBooking(int id, Guid userId)
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

        public async Task<bool> CompleteBooking(int id, Guid userId)
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

        private async Task<BookingResponseDto> CreateBookingInMemory(
            Guid userId,
            BookingCreateDto dto,
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            await InMemoryCreateLock.WaitAsync();
            try
            {
                var booking = await CreateBookingWithOverlapCheck(userId, dto, partnerCarId, startTime, endTime);
                return booking.ToBookingResponseDto();
            }
            finally
            {
                InMemoryCreateLock.Release();
            }
        }

        private async Task<Booking> CreateBookingWithOverlapCheck(
            Guid userId,
            BookingCreateDto dto,
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            if (await HasOverlappingActiveBookings(partnerCarId, startTime, endTime))
            {
                throw new InvalidOperationException("Car is already booked for this time.");
            }

            var totalHours = (decimal)(endTime - startTime).TotalHours;
            decimal? totalPrice = null;

            if (dto.PriceHour.HasValue)
            {
                totalPrice = decimal.Round(dto.PriceHour.Value * totalHours, 2, MidpointRounding.AwayFromZero);
            }

            var booking = new Booking
            {
                PartnerCarId = partnerCarId,
                UserId = userId,
                PartnerId = dto.PartnerId,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                PriceHour = dto.PriceHour,
                TotalPrice = totalPrice,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        private Task<bool> HasOverlappingActiveBookings(int partnerCarId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            return _db.Bookings
                .AnyAsync(b =>
                    b.PartnerCarId == partnerCarId &&
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                    startTime < b.EndTime &&
                    endTime > b.StartTime);
        }

        private async Task<Booking?> GetUserBookingEntity(int id, Guid userId)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            EnsureValidUserId(userId);

            return await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        }

        private static void EnsureValidDateRange(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (startTime == default)
            {
                throw new ArgumentException("StartTime is required.", nameof(startTime));
            }

            if (endTime == default)
            {
                throw new ArgumentException("EndTime is required.", nameof(endTime));
            }

            if (endTime <= startTime)
            {
                throw new ArgumentException("EndTime must be greater than StartTime.", nameof(endTime));
            }
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "id";
            }

            var normalized = sortBy.Trim().ToLowerInvariant();
            if (normalized == "startdate")
            {
                return "starttime";
            }

            if (normalized == "enddate")
            {
                return "endtime";
            }

            if (normalized is "id" or "starttime" or "endtime")
            {
                return normalized;
            }

            throw new ArgumentException("SortBy must be one of: id, startTime, endTime.", nameof(sortBy));
        }

        private static void EnsureValidUserId(Guid userId)
        {
            if (userId == Guid.Empty)
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
                BookingStatus.Confirmed => targetStatus is BookingStatus.Active or BookingStatus.Completed or BookingStatus.Canceled,
                BookingStatus.Active => targetStatus is BookingStatus.Completed,
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

        private static bool IsOverlappingConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgresException && IsOverlappingConstraintViolation(postgresException);
        }

        private static bool IsOverlappingConstraintViolation(PostgresException ex)
        {
            return ex.SqlState == PostgresErrorCodes.ExclusionViolation &&
                   string.Equals(ex.ConstraintName, "prevent_overlapping_bookings", StringComparison.Ordinal);
        }
    }
}
