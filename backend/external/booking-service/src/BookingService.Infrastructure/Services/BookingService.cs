using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Application.Mappers;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Integrations;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace BookingService.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private const int MaxSerializableRetries = 3;
        private static readonly SemaphoreSlim InMemoryCreateLock = new(1, 1);

        private readonly ApplicationDbContext _db;
        private readonly IDynamicPricingService _dynamicPricingService;
        private readonly IPaymentSyncClient _paymentSyncClient;
        private readonly IClientBookingAccessClient _clientBookingAccessClient;
        private readonly IIdentityUserReadClient _identityUserReadClient;
        private readonly IBookingCompletionTicketClient _bookingCompletionTicketClient;
        private readonly IBookingEmailClient _bookingEmailClient;
        private readonly ISubscriptionService _subscriptionService;
        private readonly PaymentServiceOptions _paymentServiceOptions;
        private readonly PendingBookingExpirationOptions _pendingBookingExpirationOptions;

        public BookingService(
            ApplicationDbContext db,
            IDynamicPricingService dynamicPricingService,
            IPaymentSyncClient paymentSyncClient,
            IClientBookingAccessClient clientBookingAccessClient,
            IIdentityUserReadClient identityUserReadClient,
            IBookingCompletionTicketClient bookingCompletionTicketClient,
            IBookingEmailClient bookingEmailClient,
            ISubscriptionService subscriptionService,
            IOptions<PaymentServiceOptions> paymentServiceOptions,
            IOptions<PendingBookingExpirationOptions> pendingBookingExpirationOptions)
        {
            _db = db;
            _dynamicPricingService = dynamicPricingService;
            _paymentSyncClient = paymentSyncClient;
            _clientBookingAccessClient = clientBookingAccessClient;
            _identityUserReadClient = identityUserReadClient;
            _bookingCompletionTicketClient = bookingCompletionTicketClient;
            _bookingEmailClient = bookingEmailClient;
            _subscriptionService = subscriptionService;
            _paymentServiceOptions = paymentServiceOptions.Value;
            _pendingBookingExpirationOptions = pendingBookingExpirationOptions.Value;
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
            await EnsureBookingActionsAllowedAsync(userId);

            var partnerCarId = dto.ResolvePartnerCarId();
            var startTime = dto.ResolveStartTime();
            var endTime = dto.ResolveEndTime();

            EnsureValidDateRange(startTime, endTime);

            var priceQuote = await _dynamicPricingService.CalculateQuoteAsync(partnerCarId, startTime, endTime);

            if (!_db.Database.IsRelational())
            {
                return await CreateBookingInMemory(
                    userId,
                    partnerCarId,
                    priceQuote.PartnerUserId,
                    priceQuote.PriceHour,
                    priceQuote.TotalPrice,
                    startTime,
                    endTime,
                    dto.UseSubscription);
            }

            for (var attempt = 1; attempt <= MaxSerializableRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    var booking = await CreateBookingWithOverlapCheck(
                        userId,
                        partnerCarId,
                        priceQuote.PartnerUserId,
                        priceQuote.PriceHour,
                        priceQuote.TotalPrice,
                        startTime,
                        endTime,
                        dto.UseSubscription);

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

        public async Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByPartnerUserId(Guid partnerUserId, CancellationToken cancellationToken = default)
        {
            EnsureValidUserId(partnerUserId);

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => booking.PartnerUserId == partnerUserId)
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

        public async Task<IReadOnlyCollection<CarAvailabilityResultDto>> CheckAvailabilityByPartnerCarIds(
            IReadOnlyCollection<int> partnerCarIds,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            EnsureValidDateRange(startTime, endTime);

            var normalizedIds = partnerCarIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (normalizedIds.Length == 0)
            {
                return [];
            }

            var requestedDuration = endTime - startTime;

            var activeBookings = await _db.Bookings
                .AsNoTracking()
                .Where(booking =>
                    normalizedIds.Contains(booking.PartnerCarId) &&
                    (booking.Status == BookingStatus.Pending ||
                     booking.Status == BookingStatus.Confirmed ||
                     booking.Status == BookingStatus.Active) &&
                    booking.EndTime > startTime)
                .OrderBy(booking => booking.PartnerCarId)
                .ThenBy(booking => booking.StartTime)
                .ToListAsync(cancellationToken);

            var bookingsByCarId = activeBookings
                .GroupBy(booking => booking.PartnerCarId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var results = new List<CarAvailabilityResultDto>(normalizedIds.Length);
            foreach (var partnerCarId in normalizedIds)
            {
                var bookings = bookingsByCarId.GetValueOrDefault(partnerCarId, []);
                var hasOverlap = bookings.Any(booking =>
                    startTime < booking.EndTime &&
                    endTime > booking.StartTime);

                var nextAvailableFrom = FindEarliestAvailableStart(bookings, startTime, requestedDuration);

                results.Add(new CarAvailabilityResultDto
                {
                    PartnerCarId = partnerCarId,
                    IsAvailable = !hasOverlap,
                    NextAvailableFrom = nextAvailableFrom
                });
            }

            return results;
        }

        public async Task<BookingPaymentStatusResponseDto> StartPayment(int id, Guid userId)
        {
            var booking = await GetRequiredUserBookingEntity(id, userId);

            if (booking.UsedSubscription)
            {
                if (booking.Status == BookingStatus.Pending)
                {
                    await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Confirmed);
                }

                return MapBookingPaymentStatus(booking, null);
            }

            if (await ExpirePendingBookingIfNeededAsync(booking))
            {
                var latestExpiredAttempt = await _paymentSyncClient.GetLatestMockPaymentAsync(booking.Id, userId);
                return MapBookingPaymentStatus(booking, latestExpiredAttempt);
            }

            MockPaymentAttemptPayload? latestAttempt = null;

            if (booking.Status == BookingStatus.Pending)
            {
                latestAttempt = await _paymentSyncClient.StartMockPaymentAsync(
                    booking.Id,
                    userId,
                    ResolveBookingPaymentAmount(booking),
                    ResolvePaymentCurrency());
            }
            else
            {
                latestAttempt = await _paymentSyncClient.GetLatestMockPaymentAsync(booking.Id, userId);
            }

            return MapBookingPaymentStatus(booking, latestAttempt);
        }

        public async Task<BookingPaymentStatusResponseDto> GetPaymentStatus(int id, Guid userId)
        {
            var booking = await GetRequiredUserBookingEntity(id, userId);

            if (booking.UsedSubscription)
            {
                if (booking.Status == BookingStatus.Pending)
                {
                    await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Confirmed);
                }

                return MapBookingPaymentStatus(booking, null);
            }

            await ExpirePendingBookingIfNeededAsync(booking);
            var latestAttempt = await _paymentSyncClient.GetLatestMockPaymentAsync(booking.Id, userId);
            return MapBookingPaymentStatus(booking, latestAttempt);
        }

        public async Task<BookingPaymentStatusResponseDto> SubmitPayment(int id, Guid userId, BookingPaymentSubmitRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var booking = await GetRequiredUserBookingEntity(id, userId);

            if (booking.UsedSubscription)
            {
                throw new InvalidOperationException("Subscription booking does not require payment submission.");
            }

            if (await ExpirePendingBookingIfNeededAsync(booking))
            {
                var latestExpiredAttempt = await _paymentSyncClient.GetLatestMockPaymentAsync(booking.Id, userId);
                return MapBookingPaymentStatus(booking, latestExpiredAttempt);
            }

            var latestAttempt = await _paymentSyncClient.SubmitMockPaymentAsync(
                booking.Id,
                userId,
                dto.SessionKey,
                dto.CardHolder,
                dto.CardNumber,
                dto.ExpiryMonth,
                dto.ExpiryYear,
                dto.Cvv);

            if (booking.Status == BookingStatus.Pending &&
                string.Equals(latestAttempt.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Confirmed);
            }

            return MapBookingPaymentStatus(booking, latestAttempt);
        }

        public async Task<bool> CancelBooking(int id, Guid userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Canceled);
            return true;
        }

        public async Task<bool> CancelBookingByPartner(int id, Guid partnerUserId)
        {
            EnsureValidUserId(partnerUserId);

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.PartnerUserId == partnerUserId);

            if (booking == null)
                return false;

            if (booking.Status == BookingStatus.Completed ||
                booking.Status == BookingStatus.Canceled)
                return false;

            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Canceled);
            return true;
        }

        public async Task<bool> ConfirmBooking(int id, Guid userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Confirmed);
            return true;
        }

        public async Task<bool> StartTrip(int id, Guid userId)
        {
            await EnsureBookingActionsAllowedAsync(userId);

            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            if (now < booking.StartTime.AddMinutes(-15))
            {
                throw new InvalidOperationException("Trip can only be started within 15 minutes before the booking start time.");
            }

            booking.TripStartedAt ??= now;
            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Active);
            return true;
        }

        public async Task<bool> CompleteBooking(int id, Guid userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            if (booking == null)
            {
                return false;
            }

            throw new InvalidOperationException("Booking completion requires the completion review form with required photos.");
        }

        public async Task<BookingCompletionSubmissionResponseDto> SubmitCompletionReview(int id, Guid userId, BookingCompletionSubmissionDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var booking = await GetRequiredUserBookingEntity(id, userId);
            if (booking.Status == BookingStatus.AwaitingReview && booking.CompletionReviewTicketId.HasValue)
            {
                return new BookingCompletionSubmissionResponseDto
                {
                    Booking = booking.ToBookingResponseDto(),
                    ReviewTicketId = booking.CompletionReviewTicketId.Value,
                    LatePenaltyAmount = CalculateLatePenaltyAmount(
                        booking,
                        booking.TripCompletedAt ?? DateTimeOffset.UtcNow)
                };
            }

            if (booking.Status != BookingStatus.Active)
            {
                throw new InvalidOperationException("Only active bookings can be submitted for completion review.");
            }

            var tripStartedAt = booking.TripStartedAt
                ?? throw new InvalidOperationException("Trip must be started before it can be completed.");

            ValidateCompletionSubmission(dto);

            var clientProfile = await _clientBookingAccessClient.GetClientProfileAsync(userId)
                ?? throw new KeyNotFoundException("Client profile not found.");
            var identityUser = await _identityUserReadClient.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException("User account not found.");

            if (string.IsNullOrWhiteSpace(identityUser.Email))
            {
                throw new InvalidOperationException("Customer email is required to submit booking completion review.");
            }

            var tripCompletedAt = DateTimeOffset.UtcNow;
            booking.TripCompletedAt = tripCompletedAt;

            var latePenaltyAmount = CalculateLatePenaltyAmount(booking, tripCompletedAt);
            var reviewTicket = await _bookingCompletionTicketClient.CreateBookingCompletionTicketAsync(
                new BookingCompletionTicketCreatePayload
                {
                    FirstName = clientProfile.FirstName,
                    LastName = clientProfile.LastName,
                    Email = identityUser.Email,
                    PhoneNumber = clientProfile.PhoneNumber,
                    BookingId = booking.Id,
                    PlannedStartTime = booking.StartTime,
                    PlannedEndTime = booking.EndTime,
                    TripStartedAt = tripStartedAt,
                    TripCompletedAt = tripCompletedAt,
                    LatePenaltyAmount = latePenaltyAmount > 0m ? latePenaltyAmount : null,
                    CompletionFrontPhotoFile = dto.CompletionFrontPhotoFile,
                    CompletionBackPhotoFile = dto.CompletionBackPhotoFile,
                    CompletionSideLeftPhotoFile = dto.CompletionSideLeftPhotoFile,
                    CompletionSideRightPhotoFile = dto.CompletionSideRightPhotoFile,
                    CompletionInteriorPhotoFile = dto.CompletionInteriorPhotoFile
                });

            booking.CompletionReviewTicketId = reviewTicket.Id;
            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.AwaitingReview);

            return new BookingCompletionSubmissionResponseDto
            {
                Booking = booking.ToBookingResponseDto(),
                ReviewTicketId = reviewTicket.Id,
                LatePenaltyAmount = latePenaltyAmount
            };
        }

        public async Task<IReadOnlyCollection<BookingChargeResponseDto>> GetBookingCharges(
            int id,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var booking = await GetRequiredUserBookingEntity(id, userId);
            var charges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);

            return charges
                .Where(charge => charge.UserId == booking.UserId)
                .OrderBy(charge => charge.CreatedAt)
                .Select(MapBookingCharge)
                .ToArray();
        }

        public async Task<BookingChargeResponseDto> PayBookingCharge(
            int id,
            long chargeId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (chargeId <= 0)
            {
                throw new ArgumentException("Charge id must be greater than zero.", nameof(chargeId));
            }

            var booking = await GetRequiredUserBookingEntity(id, userId);
            var existingCharges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);
            var existingCharge = existingCharges.FirstOrDefault(charge => charge.Id == chargeId && charge.UserId == userId)
                ?? throw new KeyNotFoundException("Booking charge not found.");

            BookingChargePayload paidCharge;
            if (IsChargeStatus(existingCharge.Status, "paid"))
            {
                paidCharge = existingCharge;
            }
            else
            {
                paidCharge = await _paymentSyncClient.MarkBookingChargePaidAsync(chargeId, cancellationToken);
            }

            var remainingCharges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);
            if (!HasPendingCharges(remainingCharges) &&
                booking.Status == BookingStatus.AwaitingReview)
            {
                await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Completed, cancellationToken);
            }

            await UnblockBookingActionsIfPossibleAsync(userId, cancellationToken);
            return MapBookingCharge(paidCharge);
        }

        public async Task<BookingResponseDto?> GetBookingById(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking => booking.Id == id)
                .SelectToBookingResponseDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task ProcessCompletionReviewApproved(
            int bookingId,
            Guid ticketId,
            decimal? latePenaltyAmount,
            string customerEmail,
            string customerFullName,
            CancellationToken cancellationToken = default)
        {
            var booking = await GetRequiredCompletionReviewBookingAsync(bookingId, ticketId, cancellationToken);
            if (booking.Status == BookingStatus.Active)
            {
                await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.AwaitingReview, cancellationToken);
            }

            if (latePenaltyAmount.HasValue && latePenaltyAmount.Value > 0m)
            {
                await EnsureBookingChargeAsync(
                    booking,
                    "LatePenalty",
                    latePenaltyAmount.Value,
                    $"Late return penalty for booking #{booking.Id}.",
                    cancellationToken);
            }

            var charges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);
            if (!HasPendingCharges(charges) && booking.Status != BookingStatus.Completed)
            {
                await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Completed, cancellationToken);
            }

            await _bookingEmailClient.SendCustomEmailAsync(
                customerEmail,
                $"Booking #{booking.Id}: completion review confirmed",
                BuildCompletionApprovedEmailText(
                    booking.Id,
                    customerFullName,
                    latePenaltyAmount,
                    HasPendingCharges(charges)),
                cancellationToken: cancellationToken);
        }

        public async Task ProcessCompletionReviewFineIssued(
            int bookingId,
            Guid ticketId,
            decimal? latePenaltyAmount,
            decimal damageFineAmount,
            string customerEmail,
            string customerFullName,
            CancellationToken cancellationToken = default)
        {
            var booking = await GetRequiredCompletionReviewBookingAsync(bookingId, ticketId, cancellationToken);
            if (booking.Status == BookingStatus.Active)
            {
                await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.AwaitingReview, cancellationToken);
            }

            if (latePenaltyAmount.HasValue && latePenaltyAmount.Value > 0m)
            {
                await EnsureBookingChargeAsync(
                    booking,
                    "LatePenalty",
                    latePenaltyAmount.Value,
                    $"Late return penalty for booking #{booking.Id}.",
                    cancellationToken);
            }

            await EnsureBookingChargeAsync(
                booking,
                "DamageFine",
                damageFineAmount,
                $"Damage fine for booking #{booking.Id}.",
                cancellationToken);

            var pendingDamageFines = await _paymentSyncClient.GetUserBookingChargesAsync(
                booking.UserId,
                "DamageFine",
                "Pending",
                cancellationToken);

            if (pendingDamageFines.Count > 0)
            {
                await _clientBookingAccessClient.SetBookingActionsBlockedAsync(
                    booking.UserId,
                    true,
                    $"Оплатите штраф по бронированию #{booking.Id}, чтобы снова создавать и начинать брони.",
                    cancellationToken);
            }
            else
            {
                await _clientBookingAccessClient.SetBookingActionsBlockedAsync(booking.UserId, false, null, cancellationToken);
            }

            await _bookingEmailClient.SendCustomEmailAsync(
                customerEmail,
                $"Booking #{booking.Id}: fine issued",
                BuildCompletionFineIssuedEmailText(
                    booking.Id,
                    customerFullName,
                    damageFineAmount,
                    latePenaltyAmount),
                cancellationToken: cancellationToken);
        }

        public async Task<BookingStatsDto> GetUserBookingStats(Guid userId, CancellationToken cancellationToken = default)
        {
            EnsureValidUserId(userId);

            var bookings = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .Select(b => new { b.Status, b.TotalPrice })
                .ToListAsync(cancellationToken);

            var totalCount = bookings.Count;
            var activeCount = bookings.Count(b =>
                b.Status == BookingStatus.Confirmed ||
                b.Status == BookingStatus.Active ||
                b.Status == BookingStatus.AwaitingReview ||
                b.Status == BookingStatus.Pending);
            var completedCount = bookings.Count(b => b.Status == BookingStatus.Completed);
            var totalSpent = bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .Sum(b => b.TotalPrice ?? 0m);

            return new BookingStatsDto
            {
                TotalCount = totalCount,
                ActiveCount = activeCount,
                CompletedCount = completedCount,
                TotalSpent = totalSpent
            };
        }

        private async Task<BookingResponseDto> CreateBookingInMemory(
            Guid userId,
            int partnerCarId,
            Guid partnerUserId,
            decimal priceHour,
            decimal totalPrice,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            bool useSubscription)
        {
            await InMemoryCreateLock.WaitAsync();
            try
            {
                var booking = await CreateBookingWithOverlapCheck(
                    userId,
                    partnerCarId,
                    partnerUserId,
                    priceHour,
                    totalPrice,
                    startTime,
                    endTime,
                    useSubscription);

                return booking.ToBookingResponseDto();
            }
            finally
            {
                InMemoryCreateLock.Release();
            }
        }

        private async Task<Booking> CreateBookingWithOverlapCheck(
            Guid userId,
            int partnerCarId,
            Guid partnerUserId,
            decimal priceHour,
            decimal quotedTotalPrice,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            bool useSubscription)
        {
            if (await HasOverlappingActiveBookings(partnerCarId, startTime, endTime))
            {
                throw new InvalidOperationException("Car is already booked for this time.");
            }

            decimal? totalPrice = null;
            int? subscriptionId = null;
            var usedSubscription = false;

            if (useSubscription)
            {
                var activeSubscription = await GetRequiredActiveSubscription(userId);

                if (activeSubscription.UsedBookings >= activeSubscription.IncludedBookings)
                {
                    throw new InvalidOperationException("No remaining bookings in active subscription.");
                }

                activeSubscription.UsedBookings += 1;
                subscriptionId = activeSubscription.Id;
                usedSubscription = true;
                totalPrice = 0m;
            }
            else
            {
                totalPrice = quotedTotalPrice;
            }

            var booking = new Booking
            {
                PartnerCarId = partnerCarId,
                UserId = userId,
                PartnerUserId = partnerUserId,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                PriceHour = priceHour,
                TotalPrice = totalPrice,
                SubscriptionId = subscriptionId,
                UsedSubscription = usedSubscription,
                CreatedAt = DateTimeOffset.UtcNow,
                TripStartedAt = null,
                TripCompletedAt = null,
                CompletionReviewTicketId = null
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

        private async Task<Subscription> GetRequiredActiveSubscription(Guid userId)
        {
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Status == "active" &&
                    x.EndDate > DateTimeOffset.UtcNow);

            if (subscription == null)
            {
                throw new InvalidOperationException("No active subscription found.");
            }

            return subscription;
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

        private async Task<Booking> GetRequiredUserBookingEntity(int id, Guid userId)
        {
            var booking = await GetUserBookingEntity(id, userId);
            return booking ?? throw new KeyNotFoundException("Booking not found.");
        }

        private async Task<Booking?> GetBookingEntityById(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            return await _db.Bookings.FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
        }

        private async Task<Booking> GetRequiredBookingEntityById(int id, CancellationToken cancellationToken = default)
        {
            var booking = await GetBookingEntityById(id, cancellationToken);
            return booking ?? throw new KeyNotFoundException("Booking not found.");
        }

        private async Task<Booking> GetRequiredCompletionReviewBookingAsync(
            int bookingId,
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            if (ticketId == Guid.Empty)
            {
                throw new ArgumentException("Ticket id is required.", nameof(ticketId));
            }

            var booking = await GetRequiredBookingEntityById(bookingId, cancellationToken);
            if (booking.CompletionReviewTicketId != ticketId)
            {
                throw new InvalidOperationException("Completion review ticket does not match this booking.");
            }

            if (booking.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.Canceled)
            {
                throw new InvalidOperationException("Booking is not in a completion review state.");
            }

            return booking;
        }

        private async Task<BookingChargePayload> EnsureBookingChargeAsync(
            Booking booking,
            string chargeType,
            decimal amount,
            string description,
            CancellationToken cancellationToken = default)
        {
            var normalizedAmount = RoundCurrency(amount);
            var existingCharges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);
            var existingCharge = existingCharges.FirstOrDefault(charge =>
                string.Equals(charge.ChargeType, chargeType, StringComparison.OrdinalIgnoreCase));

            if (existingCharge is not null)
            {
                if (Math.Abs(existingCharge.Amount - normalizedAmount) > 0.01m)
                {
                    throw new InvalidOperationException(
                        $"Existing booking charge '{chargeType}' has amount {existingCharge.Amount}, expected {normalizedAmount}.");
                }

                return existingCharge;
            }

            return await _paymentSyncClient.CreateBookingChargeAsync(
                booking.Id,
                booking.UserId,
                booking.PartnerUserId,
                chargeType,
                normalizedAmount,
                description,
                cancellationToken);
        }

        private async Task UnblockBookingActionsIfPossibleAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var pendingDamageFines = await _paymentSyncClient.GetUserBookingChargesAsync(
                userId,
                "DamageFine",
                "Pending",
                cancellationToken);

            if (pendingDamageFines.Count > 0)
            {
                return;
            }

            await _clientBookingAccessClient.SetBookingActionsBlockedAsync(userId, false, null, cancellationToken);
        }

        private static BookingChargeResponseDto MapBookingCharge(BookingChargePayload charge)
        {
            return new BookingChargeResponseDto
            {
                Id = charge.Id,
                BookingId = charge.BookingId,
                ChargeType = charge.ChargeType,
                Amount = charge.Amount,
                PartnerShareAmount = charge.PartnerShareAmount,
                Currency = charge.Currency,
                Status = charge.Status,
                Description = charge.Description,
                CreatedAt = charge.CreatedAt,
                UpdatedAt = charge.UpdatedAt,
                PaidAt = charge.PaidAt,
                CanceledAt = charge.CanceledAt
            };
        }

        private static void ValidateCompletionSubmission(BookingCompletionSubmissionDto dto)
        {
            ValidateRequiredFile(dto.CompletionFrontPhotoFile, nameof(dto.CompletionFrontPhotoFile));
            ValidateRequiredFile(dto.CompletionBackPhotoFile, nameof(dto.CompletionBackPhotoFile));
            ValidateRequiredFile(dto.CompletionSideLeftPhotoFile, nameof(dto.CompletionSideLeftPhotoFile));
            ValidateRequiredFile(dto.CompletionSideRightPhotoFile, nameof(dto.CompletionSideRightPhotoFile));
            ValidateRequiredFile(dto.CompletionInteriorPhotoFile, nameof(dto.CompletionInteriorPhotoFile));
        }

        private static void ValidateRequiredFile(FileUploadPayload file, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                throw new ArgumentException("File name is required.", parameterName);
            }

            if (file.Content.Length == 0)
            {
                throw new ArgumentException("File content is required.", parameterName);
            }
        }

        private static decimal CalculateLatePenaltyAmount(Booking booking, DateTimeOffset tripCompletedAt)
        {
            if (tripCompletedAt <= booking.EndTime)
            {
                return 0m;
            }

            var hourlyRate = ResolveLatePenaltyHourlyRate(booking);
            var overdueMinutes = (decimal)(tripCompletedAt - booking.EndTime).TotalMinutes;
            if (overdueMinutes <= 0m)
            {
                return 0m;
            }

            return RoundCurrency(hourlyRate * 2m * overdueMinutes / 60m);
        }

        private static decimal ResolveLatePenaltyHourlyRate(Booking booking)
        {
            if (booking.PriceHour.HasValue && booking.PriceHour.Value > 0m)
            {
                return booking.PriceHour.Value;
            }

            if (booking.TotalPrice.HasValue && booking.TotalPrice.Value > 0m)
            {
                var plannedHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
                if (plannedHours > 0m)
                {
                    return RoundCurrency(booking.TotalPrice.Value / plannedHours);
                }
            }

            throw new InvalidOperationException("Booking hourly rate is required to calculate late penalty.");
        }

        private static bool HasPendingCharges(IReadOnlyCollection<BookingChargePayload> charges)
        {
            return charges.Any(charge => IsChargeStatus(charge.Status, "pending"));
        }

        private static bool IsChargeStatus(string? actualStatus, string expectedStatus)
        {
            return string.Equals(actualStatus?.Trim(), expectedStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        private static string BuildCompletionApprovedEmailText(
            int bookingId,
            string customerFullName,
            decimal? latePenaltyAmount,
            bool hasPendingCharges)
        {
            var normalizedName = string.IsNullOrWhiteSpace(customerFullName) ? "Customer" : customerFullName.Trim();
            if (latePenaltyAmount.HasValue && latePenaltyAmount.Value > 0m && hasPendingCharges)
            {
                return $"{normalizedName}, завершение поездки по бронированию #{bookingId} подтверждено. " +
                       $"Начислена пеня за поздний возврат: {RoundCurrency(latePenaltyAmount.Value):0.00} KZT. " +
                       "После оплаты начисления бронь будет переведена в completed.";
            }

            return $"{normalizedName}, завершение поездки по бронированию #{bookingId} подтверждено. " +
                   "Бронь успешно переведена в статус completed.";
        }

        private static string BuildCompletionFineIssuedEmailText(
            int bookingId,
            string customerFullName,
            decimal damageFineAmount,
            decimal? latePenaltyAmount)
        {
            var normalizedName = string.IsNullOrWhiteSpace(customerFullName) ? "Customer" : customerFullName.Trim();
            var fineText = $"{normalizedName}, по бронированию #{bookingId} начислен штраф за повреждение: {RoundCurrency(damageFineAmount):0.00} KZT.";
            if (latePenaltyAmount.HasValue && latePenaltyAmount.Value > 0m)
            {
                fineText += $" Дополнительно начислена пеня за поздний возврат: {RoundCurrency(latePenaltyAmount.Value):0.00} KZT.";
            }

            return fineText + " Пока штраф не будет оплачен, создание и начало новых броней будут заблокированы.";
        }

        private async Task PersistStatusTransitionWithPaymentOutbox(
            Booking booking,
            BookingStatus targetStatus,
            CancellationToken cancellationToken = default)
        {
            var statusChanged = TryApplyStatusTransition(booking, targetStatus);
            var outboxMessage = CreatePaymentSyncOutboxMessage(booking, targetStatus);
            if (outboxMessage is null)
            {
                if (statusChanged || _db.Entry(booking).State == EntityState.Modified)
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return;
            }

            var outboxExists = await _db.PaymentSyncOutboxMessages
                .AnyAsync(message => message.EventKey == outboxMessage.EventKey, cancellationToken);

            if (!statusChanged && outboxExists)
            {
                return;
            }

            if (!outboxExists)
            {
                _db.PaymentSyncOutboxMessages.Add(outboxMessage);
            }

            if (statusChanged || !outboxExists)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
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

        private static DateTimeOffset FindEarliestAvailableStart(
            IReadOnlyCollection<Booking> bookings,
            DateTimeOffset requestedStartTime,
            TimeSpan requestedDuration)
        {
            var cursor = requestedStartTime;
            foreach (var booking in bookings.OrderBy(item => item.StartTime))
            {
                if (cursor + requestedDuration <= booking.StartTime)
                {
                    return cursor;
                }

                if (booking.EndTime > cursor)
                {
                    cursor = booking.EndTime;
                }
            }

            return cursor;
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "id";
            }

            var normalized = sortBy.Trim().ToLowerInvariant();
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

        private static bool TryApplyStatusTransition(Booking booking, BookingStatus targetStatus)
        {
            if (booking.Status == targetStatus)
            {
                return false;
            }

            var isAllowed = booking.Status switch
            {
                BookingStatus.Pending => targetStatus is BookingStatus.Confirmed or BookingStatus.Canceled,
                BookingStatus.Confirmed => targetStatus is BookingStatus.Active or BookingStatus.Completed or BookingStatus.Canceled,
                BookingStatus.Active => targetStatus is BookingStatus.AwaitingReview or BookingStatus.Completed,
                BookingStatus.AwaitingReview => targetStatus is BookingStatus.Completed or BookingStatus.Canceled,
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
            return true;
        }

        private decimal ResolveBookingPaymentAmount(Booking booking)
        {
            if (!booking.TotalPrice.HasValue || booking.TotalPrice.Value <= 0m)
            {
                throw new InvalidOperationException("Booking total price must be greater than zero before payment can start.");
            }

            return booking.TotalPrice.Value;
        }

        private string ResolvePaymentCurrency()
        {
            if (string.IsNullOrWhiteSpace(_paymentServiceOptions.Currency))
            {
                throw new InvalidOperationException("PaymentService:Currency configuration is required.");
            }

            return _paymentServiceOptions.Currency;
        }

        private BookingPaymentStatusResponseDto MapBookingPaymentStatus(
            Booking booking,
            MockPaymentAttemptPayload? latestAttempt)
        {
            var normalizedPaymentStatus = ResolvePaymentStatus(booking, latestAttempt);
            var canRetry = booking.Status == BookingStatus.Pending &&
                           normalizedPaymentStatus is "not_started" or "failed" or "expired";
            DateTimeOffset? bookingExpiresAt = booking.Status == BookingStatus.Pending
                ? booking.CreatedAt.AddMinutes(_pendingBookingExpirationOptions.TtlMinutes)
                : null;

            return new BookingPaymentStatusResponseDto
            {
                BookingId = booking.Id,
                BookingStatus = booking.Status.ToString().ToLowerInvariant(),
                PaymentStatus = normalizedPaymentStatus,
                PaymentAttemptId = latestAttempt?.Id,
                SessionKey = latestAttempt?.SessionKey,
                Amount = latestAttempt?.Amount ?? booking.TotalPrice,
                Currency = latestAttempt?.Currency ?? ResolvePaymentCurrency(),
                CardHolder = latestAttempt?.CardHolder,
                CardLast4 = latestAttempt?.CardLast4,
                FailureReason = latestAttempt?.FailureReason,
                BookingCreatedAt = booking.CreatedAt,
                BookingExpiresAt = bookingExpiresAt,
                PaymentCreatedAt = latestAttempt?.CreatedAt,
                PaymentUpdatedAt = latestAttempt?.UpdatedAt,
                PaymentCompletedAt = latestAttempt?.CompletedAt,
                PaymentExpiresAt = latestAttempt?.ExpiresAt,
                RequiresInput = booking.Status == BookingStatus.Pending &&
                                normalizedPaymentStatus is "not_started" or "started",
                CanRetry = canRetry
            };
        }

        private static string ResolvePaymentStatus(Booking booking, MockPaymentAttemptPayload? latestAttempt)
        {
            if (latestAttempt is not null)
            {
                return latestAttempt.Status.Trim().ToLowerInvariant();
            }

            return booking.Status switch
            {
                BookingStatus.Pending => "not_started",
                BookingStatus.Canceled => "canceled",
                BookingStatus.Confirmed or BookingStatus.Active or BookingStatus.AwaitingReview or BookingStatus.Completed => "succeeded",
                _ => "not_started"
            };
        }

        private async Task EnsureBookingActionsAllowedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var bookingAccess = await _clientBookingAccessClient.GetBookingAccessAsync(userId, cancellationToken);
            if (bookingAccess is null || !bookingAccess.BookingActionsBlocked)
            {
                return;
            }

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(bookingAccess.BookingBlockReason)
                    ? "Booking actions are temporarily blocked for this user."
                    : bookingAccess.BookingBlockReason);
        }

        private async Task<bool> ExpirePendingBookingIfNeededAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            if (booking.Status != BookingStatus.Pending)
            {
                return false;
            }

            if (!IsPendingBookingExpired(booking))
            {
                return false;
            }

            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Canceled, cancellationToken);
            return true;
        }

        private bool IsPendingBookingExpired(Booking booking)
        {
            return booking.CreatedAt.AddMinutes(_pendingBookingExpirationOptions.TtlMinutes) <= DateTimeOffset.UtcNow;
        }

        private static PaymentSyncOutboxMessage? CreatePaymentSyncOutboxMessage(Booking booking, BookingStatus targetStatus)
        {
            var now = DateTimeOffset.UtcNow;
            var eventType = targetStatus switch
            {
                BookingStatus.Confirmed => PaymentSyncOutboxEventTypes.BookingConfirmed,
                BookingStatus.Canceled => PaymentSyncOutboxEventTypes.BookingCanceled,
                BookingStatus.Completed => PaymentSyncOutboxEventTypes.BookingCompleted,
                _ => null
            };
            if (eventType is null)
            {
                return null;
            }

            var payload = targetStatus switch
            {
                BookingStatus.Confirmed => new PaymentSyncOutboxPayload
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    PartnerUserId = booking.PartnerUserId,
                    PartnerCarId = booking.PartnerCarId,
                    PriceHour = booking.PriceHour,
                    TotalPrice = booking.TotalPrice
                },
                BookingStatus.Canceled or BookingStatus.Completed => new PaymentSyncOutboxPayload
                {
                    BookingId = booking.Id
                },
                _ => throw new InvalidOperationException($"Booking status {targetStatus} does not produce a payment outbox event.")
            };

            return new PaymentSyncOutboxMessage
            {
                BookingId = booking.Id,
                EventKey = $"booking:{booking.Id}:status:{targetStatus.ToString().ToLowerInvariant()}",
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = now,
                NextAttemptAt = now
            };
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
