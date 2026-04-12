using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs;
using BookingService.Application.DTOs.Common;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Application.Mappers;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Domain.ValueObjects;
using BookingService.Infrastructure.Integrations;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace BookingService.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private const int MaxSerializableRetries = 3;
        private const string CancellationActorClient = "client";
        private const string CancellationActorPartner = "partner";
        private const string CancellationActorManager = "manager";
        private static readonly SemaphoreSlim InMemoryCreateLock = new(1, 1);

        private readonly ApplicationDbContext _db;
        private readonly IDynamicPricingService _dynamicPricingService;
        private readonly IPartnerCarReadClient _partnerCarReadClient;
        private readonly ICarCommentWriteClient _carCommentWriteClient;
        private readonly IPaymentSyncClient _paymentSyncClient;
        private readonly IClientBookingAccessClient _clientBookingAccessClient;
        private readonly IIdentityUserReadClient _identityUserReadClient;
        private readonly IPartnerProfileReadClient _partnerProfileReadClient;
        private readonly IBookingCompletionTicketClient _bookingCompletionTicketClient;
        private readonly IPartnerBookingCancellationTicketClient _partnerBookingCancellationTicketClient;
        private readonly IBookingEmailClient _bookingEmailClient;
        private readonly PaymentServiceOptions _paymentServiceOptions;
        private readonly PendingBookingExpirationOptions _pendingBookingExpirationOptions;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ApplicationDbContext db,
            IDynamicPricingService dynamicPricingService,
            IPartnerCarReadClient partnerCarReadClient,
            ICarCommentWriteClient carCommentWriteClient,
            IPaymentSyncClient paymentSyncClient,
            IClientBookingAccessClient clientBookingAccessClient,
            IIdentityUserReadClient identityUserReadClient,
            IPartnerProfileReadClient partnerProfileReadClient,
            IBookingCompletionTicketClient bookingCompletionTicketClient,
            IPartnerBookingCancellationTicketClient partnerBookingCancellationTicketClient,
            IBookingEmailClient bookingEmailClient,
            IOptions<PaymentServiceOptions> paymentServiceOptions,
            IOptions<PendingBookingExpirationOptions> pendingBookingExpirationOptions,
            ILogger<BookingService> logger)
        {
            _db = db;
            _dynamicPricingService = dynamicPricingService;
            _partnerCarReadClient = partnerCarReadClient;
            _carCommentWriteClient = carCommentWriteClient;
            _paymentSyncClient = paymentSyncClient;
            _clientBookingAccessClient = clientBookingAccessClient;
            _identityUserReadClient = identityUserReadClient;
            _partnerProfileReadClient = partnerProfileReadClient;
            _bookingCompletionTicketClient = bookingCompletionTicketClient;
            _partnerBookingCancellationTicketClient = partnerBookingCancellationTicketClient;
            _bookingEmailClient = bookingEmailClient;
            _paymentServiceOptions = paymentServiceOptions.Value;
            _pendingBookingExpirationOptions = pendingBookingExpirationOptions.Value;
            _logger = logger;
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
                    priceQuote,
                    startTime,
                    endTime);
            }

            for (var attempt = 1; attempt <= MaxSerializableRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    var booking = await CreateBookingWithOverlapCheck(
                        userId,
                        partnerCarId,
                        priceQuote,
                        startTime,
                        endTime);

                    await transaction.CommitAsync();
                    await EnsureMockPaymentSessionStartedAsync(booking);
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

        public async Task<PagedResult<BookingResponseDto>> GetAllBookingsPaginated(BookingQueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            var sortBy = NormalizeSortBy(queryParams.SortBy);
            var isDescending = string.Equals(queryParams.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<Booking> query = _db.Bookings.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParams.Status) &&
                Enum.TryParse<BookingStatus>(queryParams.Status, true, out var statusFilter))
            {
                query = query.Where(b => b.Status == statusFilter);
            }

            if (queryParams.UserId.HasValue)
            {
                query = query.Where(b => b.UserId == queryParams.UserId.Value);
            }

            if (queryParams.PartnerUserId.HasValue)
            {
                query = query.Where(b => b.PartnerUserId == queryParams.PartnerUserId.Value);
            }

            if (queryParams.PartnerCarId.HasValue)
            {
                query = query.Where(b => b.PartnerCarId == queryParams.PartnerCarId.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var q = queryParams.Search.Trim().ToLower();
                var isIdSearch = int.TryParse(queryParams.Search.Trim(), out var searchId);
                query = query.Where(b =>
                    (isIdSearch && b.Id == searchId) ||
                    (b.CarBrand != null && b.CarBrand.ToLower().Contains(q)) ||
                    (b.CarModel != null && b.CarModel.ToLower().Contains(q)));
            }

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

            await ExpirePendingBookingIfNeededAsync(booking);
            var latestAttempt = await _paymentSyncClient.GetLatestMockPaymentAsync(booking.Id, userId);
            return MapBookingPaymentStatus(booking, latestAttempt);
        }

        public async Task<BookingPaymentStatusResponseDto> SubmitPayment(int id, Guid userId, BookingPaymentSubmitRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var booking = await GetRequiredUserBookingEntity(id, userId);

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

            await CancelBookingWithMetadataAsync(
                booking,
                CancellationActorClient,
                "Бронирование отменено клиентом.",
                notifyCustomer: false);
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

            await CancelBookingWithMetadataAsync(
                booking,
                CancellationActorPartner,
                "Бронирование отменено партнером.",
                notifyCustomer: true);
            return true;
        }

        public async Task<PartnerBookingCancellationRequestResultDto> RequestPartnerCancellation(
            int id,
            Guid partnerUserId,
            string requesterEmail,
            string reason,
            CancellationToken cancellationToken = default)
        {
            EnsureValidUserId(partnerUserId);

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(
                    value => value.Id == id && value.PartnerUserId == partnerUserId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status is BookingStatus.Completed or BookingStatus.Canceled)
            {
                throw new InvalidOperationException($"Booking cannot be canceled because its status is '{booking.Status}'.");
            }

            if (booking.Status is not BookingStatus.Pending and not BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Partner cancellation requests are allowed only for pending or confirmed bookings.");
            }

            if (booking.PartnerCancellationTicketId.HasValue)
            {
                return new PartnerBookingCancellationRequestResultDto
                {
                    ReviewTicketId = booking.PartnerCancellationTicketId.Value,
                    AlreadyPending = true,
                    Booking = booking.ToBookingResponseDto()
                };
            }

            var normalizedRequesterEmail = NormalizeRequiredText(
                requesterEmail,
                nameof(requesterEmail),
                255).ToLowerInvariant();
            var normalizedReason = NormalizeRequiredText(reason, nameof(reason), 1000);
            var requesterProfile = await ResolvePartnerCancellationRequesterProfileAsync(
                partnerUserId,
                booking.PartnerName,
                cancellationToken);

            var reviewTicket = await _partnerBookingCancellationTicketClient.CreatePartnerBookingCancellationTicketAsync(
                new PartnerBookingCancellationTicketCreatePayload
                {
                    FirstName = requesterProfile.FirstName,
                    LastName = requesterProfile.LastName,
                    Email = normalizedRequesterEmail,
                    PhoneNumber = requesterProfile.PhoneNumber,
                    RelatedPartnerUserId = partnerUserId,
                    BookingId = booking.Id,
                    CarBrand = booking.CarBrand ?? string.Empty,
                    CarModel = booking.CarModel ?? string.Empty,
                    BookingStatus = booking.Status.ToString().ToLowerInvariant(),
                    BookingStartTime = booking.StartTime,
                    BookingEndTime = booking.EndTime,
                    PartnerReason = normalizedReason
                },
                cancellationToken);

            booking.PartnerCancellationTicketId = reviewTicket.Id;
            booking.PartnerCancellationRequestedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            await NotifyPartnerAboutCancellationRequestAsync(
                normalizedRequesterEmail,
                BuildPersonName(requesterProfile.FirstName, requesterProfile.LastName),
                booking,
                normalizedReason,
                cancellationToken);

            return new PartnerBookingCancellationRequestResultDto
            {
                ReviewTicketId = reviewTicket.Id,
                AlreadyPending = false,
                Booking = booking.ToBookingResponseDto()
            };
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

        public async Task<BookingCarCommentSubmissionResponseDto> SubmitCarComment(
            int id,
            Guid userId,
            BookingCarCommentCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var booking = await GetRequiredUserBookingEntity(id, userId);
            if (booking.Status != BookingStatus.Completed)
            {
                throw new InvalidOperationException("Car comment can only be submitted for completed bookings.");
            }

            var normalizedContent = NormalizeRequiredText(dto.Content, nameof(dto.Content), 4000);
            ValidateCommentRating(dto.Rating);

            var createdComment = await _carCommentWriteClient.CreateForCompletedBookingAsync(
                new CreateCompletedBookingCarCommentPayload
                {
                    BookingId = booking.Id,
                    PartnerCarId = booking.PartnerCarId,
                    UserId = userId.ToString("D"),
                    UserName = await ResolveCommentAuthorNameAsync(userId, cancellationToken),
                    Rating = dto.Rating,
                    Content = normalizedContent
                },
                cancellationToken);

            var submittedAt = NormalizeCommentSubmittedAt(createdComment.CreatedOn);
            if (booking.CarCommentId != createdComment.Id || booking.CarCommentSubmittedAt != submittedAt)
            {
                booking.CarCommentId = createdComment.Id;
                booking.CarCommentSubmittedAt = submittedAt;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new BookingCarCommentSubmissionResponseDto
            {
                Booking = booking.ToBookingResponseDto(),
                CommentId = createdComment.Id,
                SubmittedAt = submittedAt
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

        public async Task<IReadOnlyCollection<BookingChargeResponseDto>> GetAllBookingCharges(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking id must be greater than zero.", nameof(id));
            }

            var booking = await _db.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Booking not found.");

            var charges = await _paymentSyncClient.GetBookingChargesAsync(booking.Id, cancellationToken);

            return charges
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

        public async Task<bool> CancelBookingByAdmin(
            int id,
            string? cancellationReason = null,
            CancellationToken cancellationToken = default)
        {
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            if (booking == null)
                return false;

            if (booking.Status == BookingStatus.Completed ||
                booking.Status == BookingStatus.Canceled)
                return false;

            await CancelBookingWithMetadataAsync(
                booking,
                CancellationActorManager,
                string.IsNullOrWhiteSpace(cancellationReason)
                    ? "Бронирование отменено менеджером."
                    : cancellationReason,
                notifyCustomer: true,
                cancellationToken);
            return true;
        }

        public async Task<int> CancelActiveBookingsByUserAsync(
            Guid userId,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            EnsureValidUserId(userId);

            var activeBookings = await _db.Bookings
                .Where(b =>
                    b.UserId == userId &&
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Confirmed ||
                     b.Status == BookingStatus.Active))
                .ToListAsync(cancellationToken);

            if (activeBookings.Count == 0)
                return 0;

            var cancelReason = string.IsNullOrWhiteSpace(reason)
                ? "Бронирование отменено в связи с удалением аккаунта."
                : reason;

            foreach (var booking in activeBookings)
            {
                await CancelBookingWithMetadataAsync(
                    booking,
                    CancellationActorManager,
                    cancelReason,
                    notifyCustomer: false,
                    cancellationToken);
            }

            return activeBookings.Count;
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
            string fineComment,
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
                BuildDamageFineDescription(booking.Id, fineComment),
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
                    latePenaltyAmount,
                    fineComment),
                cancellationToken: cancellationToken);
        }

        public async Task ProcessPartnerCancellationApproved(
            int bookingId,
            Guid ticketId,
            string partnerReason,
            CancellationToken cancellationToken = default)
        {
            if (ticketId == Guid.Empty)
            {
                throw new ArgumentException("Ticket id is required.", nameof(ticketId));
            }

            var booking = await GetRequiredBookingEntityById(bookingId, cancellationToken);
            if (booking.PartnerCancellationTicketId.HasValue &&
                booking.PartnerCancellationTicketId.Value != ticketId)
            {
                throw new InvalidOperationException("Partner cancellation request does not match the booking.");
            }

            if (booking.Status is BookingStatus.Canceled or BookingStatus.Completed)
            {
                return;
            }

            booking.PartnerCancellationTicketId ??= ticketId;
            booking.PartnerCancellationRequestedAt ??= DateTimeOffset.UtcNow;

            await CancelBookingWithMetadataAsync(
                booking,
                CancellationActorPartner,
                string.IsNullOrWhiteSpace(partnerReason)
                    ? "Бронирование отменено по запросу партнера."
                    : partnerReason,
                notifyCustomer: true,
                cancellationToken);
        }

        public async Task ProcessPartnerCancellationRejected(
            int bookingId,
            Guid ticketId,
            string decisionReason,
            CancellationToken cancellationToken = default)
        {
            _ = NormalizeRequiredText(decisionReason, nameof(decisionReason), 1000);

            if (ticketId == Guid.Empty)
            {
                throw new ArgumentException("Ticket id is required.", nameof(ticketId));
            }

            var booking = await GetRequiredBookingEntityById(bookingId, cancellationToken);
            if (booking.PartnerCancellationTicketId.HasValue &&
                booking.PartnerCancellationTicketId.Value != ticketId)
            {
                throw new InvalidOperationException("Partner cancellation request does not match the booking.");
            }

            booking.PartnerCancellationTicketId = null;
            booking.PartnerCancellationRequestedAt = null;
            await _db.SaveChangesAsync(cancellationToken);
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
            BookingPriceQuoteDto priceQuote,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            await InMemoryCreateLock.WaitAsync();
            try
            {
                var booking = await CreateBookingWithOverlapCheck(
                    userId,
                    partnerCarId,
                    priceQuote,
                    startTime,
                    endTime);

                await EnsureMockPaymentSessionStartedAsync(booking);
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
            BookingPriceQuoteDto priceQuote,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            if (await HasOverlappingActiveBookings(partnerCarId, startTime, endTime))
            {
                throw new InvalidOperationException("Car is already booked for this time.");
            }

            if (await HasOverlappingUserBookings(userId, startTime, endTime))
            {
                throw new InvalidOperationException("You already have a booking for this time period.");
            }

            var displaySnapshot = await GetBookingDisplaySnapshotAsync(
                partnerCarId,
                priceQuote.PartnerUserId);

            var booking = new Booking
            {
                PartnerCarId = partnerCarId,
                UserId = userId,
                PartnerUserId = priceQuote.PartnerUserId,
                CarBrand = displaySnapshot.CarBrand,
                CarModel = displaySnapshot.CarModel,
                PartnerName = displaySnapshot.PartnerName,
                CoverImageUrl = displaySnapshot.CoverImageUrl,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                PriceHour = priceQuote.PriceHour,
                TotalPrice = priceQuote.TotalPrice,
                CreatedAt = DateTimeOffset.UtcNow,
                TripStartedAt = null,
                TripCompletedAt = null,
                CompletionReviewTicketId = null,
                PricingBreakdown = CreatePricingBreakdownSnapshot(priceQuote),
                ImageUrls = displaySnapshot.ImageUrls
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        private static BookingPricingBreakdownSnapshot CreatePricingBreakdownSnapshot(BookingPriceQuoteDto priceQuote)
        {
            return new BookingPricingBreakdownSnapshot
            {
                QuotedAtUtc = priceQuote.QuotedAtUtc,
                MarketValueKzt = priceQuote.MarketValueKzt,
                Rating = priceQuote.Rating,
                CurrentAvailableCarsCount = priceQuote.CurrentAvailableCarsCount,
                DaysBeforeBooking = priceQuote.DaysBeforeBooking,
                BillableHours = priceQuote.BillableHours,
                RatingCoefficient = priceQuote.RatingCoefficient,
                AdvanceBookingCoefficient = priceQuote.AdvanceBookingCoefficient,
                AvailabilityCoefficient = priceQuote.AvailabilityCoefficient,
                QuotedPriceHour = priceQuote.PriceHour,
                QuotedTotalPrice = priceQuote.TotalPrice,
                Currency = priceQuote.Currency,
                IsMarketValueStale = priceQuote.IsMarketValueStale
            };
        }

        private async Task<BookingDisplaySnapshot> GetBookingDisplaySnapshotAsync(
            int partnerCarId,
            Guid partnerUserId,
            CancellationToken cancellationToken = default)
        {
            var partnerCarTask = _partnerCarReadClient.GetSnapshotAsync(partnerCarId, cancellationToken);
            var partnerProfileTask = _partnerProfileReadClient.GetPublicProfileByRelatedUserIdAsync(
                partnerUserId,
                cancellationToken);

            var partnerCar = await partnerCarTask
                ?? throw new KeyNotFoundException("Partner car snapshot not found.");

            if (partnerCar.PartnerUserId != partnerUserId)
            {
                throw new InvalidOperationException("Partner car snapshot does not match pricing partner user.");
            }

            PartnerPublicProfilePayload? partnerProfile = null;
            try
            {
                partnerProfile = await partnerProfileTask;
            }
            catch
            {
                partnerProfile = null;
            }

            var partnerName = string.IsNullOrWhiteSpace(partnerProfile?.CarrierName)
                ? "Партнер"
                : partnerProfile!.CarrierName.Trim();

            return new BookingDisplaySnapshot(
                string.IsNullOrWhiteSpace(partnerCar.CarBrand) ? string.Empty : partnerCar.CarBrand.Trim(),
                string.IsNullOrWhiteSpace(partnerCar.CarModel) ? string.Empty : partnerCar.CarModel.Trim(),
                partnerName,
                NormalizeOptionalUrl(partnerCar.CoverImageUrl),
                NormalizeImageUrls(partnerCar.ImageUrls, partnerCar.CoverImageUrl));
        }

        private async Task<(string FirstName, string LastName, string PhoneNumber)> ResolvePartnerCancellationRequesterProfileAsync(
            Guid partnerUserId,
            string? fallbackFullName,
            CancellationToken cancellationToken)
        {
            PartnerPublicProfilePayload? partnerProfile = null;
            try
            {
                partnerProfile = await _partnerProfileReadClient.GetPublicProfileByRelatedUserIdAsync(
                    partnerUserId,
                    cancellationToken);
            }
            catch
            {
                partnerProfile = null;
            }

            var (fallbackFirstName, fallbackLastName) = SplitPersonName(
                string.IsNullOrWhiteSpace(partnerProfile?.CarrierName)
                    ? fallbackFullName
                    : partnerProfile!.CarrierName);

            var firstName = string.IsNullOrWhiteSpace(partnerProfile?.OwnerFirstName)
                ? fallbackFirstName
                : partnerProfile.OwnerFirstName.Trim();
            var lastName = string.IsNullOrWhiteSpace(partnerProfile?.OwnerLastName)
                ? fallbackLastName
                : partnerProfile.OwnerLastName.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(partnerProfile?.PhoneNumber)
                ? "Не указан"
                : partnerProfile.PhoneNumber.Trim();

            if (string.IsNullOrWhiteSpace(firstName))
            {
                firstName = "Партнер";
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                lastName = "Пользователь";
            }

            return (firstName, lastName, phoneNumber);
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

        private Task<bool> HasOverlappingUserBookings(Guid userId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            return _db.Bookings
                .AnyAsync(b =>
                    b.UserId == userId &&
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

        private async Task<string> ResolveCommentAuthorNameAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var clientProfile = await _clientBookingAccessClient.GetClientProfileAsync(userId, cancellationToken);
            var fullName = BuildPersonName(clientProfile?.FirstName, clientProfile?.LastName);
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                return fullName;
            }

            var identityUser = await _identityUserReadClient.GetUserByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException("User account not found.");

            var userName = identityUser.Username?.Trim();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName;
            }

            throw new InvalidOperationException("User name is required to submit booking car comment.");
        }

        private static string BuildPersonName(string? firstName, string? lastName)
        {
            var parts = new[] { firstName?.Trim(), lastName?.Trim() }
                .Where(part => !string.IsNullOrWhiteSpace(part));

            return string.Join(" ", parts);
        }

        private static (string FirstName, string LastName) SplitPersonName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty);
            }

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }

            return (parts[0], string.Join(' ', parts.Skip(1)));
        }

        private static string NormalizeRequiredText(string? value, string parameterName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} is required.", parameterName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{parameterName} length must not exceed {maxLength}.",
                    parameterName);
            }

            return normalized;
        }

        private static string? NormalizeOptionalText(string? value, string parameterName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{parameterName} length must not exceed {maxLength}.",
                    parameterName);
            }

            return normalized;
        }

        private static void ValidateCommentRating(int rating)
        {
            if (rating is < 1 or > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
            }
        }

        private static DateTimeOffset NormalizeCommentSubmittedAt(DateTime createdOn)
        {
            return createdOn.Kind switch
            {
                DateTimeKind.Utc => new DateTimeOffset(createdOn, TimeSpan.Zero),
                DateTimeKind.Local => createdOn.ToUniversalTime(),
                _ => new DateTimeOffset(DateTime.SpecifyKind(createdOn, DateTimeKind.Utc), TimeSpan.Zero)
            };
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

        private static string? NormalizeOptionalUrl(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static IReadOnlyList<string> NormalizeImageUrls(
            IReadOnlyList<string>? imageUrls,
            string? coverImageUrl)
        {
            var normalized = (imageUrls ?? [])
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var normalizedCover = NormalizeOptionalUrl(coverImageUrl);
            if (normalizedCover is not null)
            {
                normalized.RemoveAll(item => string.Equals(item, normalizedCover, StringComparison.Ordinal));
                normalized.Insert(0, normalizedCover);
            }

            return normalized;
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
            decimal? latePenaltyAmount,
            string? fineComment)
        {
            var normalizedName = string.IsNullOrWhiteSpace(customerFullName) ? "Customer" : customerFullName.Trim();
            var fineText = $"{normalizedName}, по бронированию #{bookingId} начислен штраф за повреждение: {RoundCurrency(damageFineAmount):0.00} KZT.";
            if (latePenaltyAmount.HasValue && latePenaltyAmount.Value > 0m)
            {
                fineText += $" Дополнительно начислена пеня за поздний возврат: {RoundCurrency(latePenaltyAmount.Value):0.00} KZT.";
            }

            if (!string.IsNullOrWhiteSpace(fineComment))
            {
                fineText += $" Комментарий менеджера: {fineComment.Trim()}.";
            }

            return fineText + " Пока штраф не будет оплачен, создание и начало новых броней будут заблокированы.";
        }

        private static string BuildDamageFineDescription(int bookingId, string? fineComment)
        {
            if (string.IsNullOrWhiteSpace(fineComment))
            {
                return $"Damage fine for booking #{bookingId}.";
            }

            return $"Damage fine for booking #{bookingId}: {fineComment.Trim()}";
        }

        private async Task CancelBookingWithMetadataAsync(
            Booking booking,
            string cancellationActor,
            string cancellationReason,
            bool notifyCustomer,
            CancellationToken cancellationToken = default)
        {
            var previousStatus = booking.Status;
            booking.CancellationActor = NormalizeCancellationActor(cancellationActor);
            booking.CancellationReason = NormalizeCancellationReason(cancellationReason, booking.CancellationActor);

            await PersistStatusTransitionWithPaymentOutbox(booking, BookingStatus.Canceled, cancellationToken);

            if (notifyCustomer)
            {
                await NotifyCustomerAboutCancellationAsync(
                    booking,
                    previousStatus,
                    booking.CancellationActor,
                    booking.CancellationReason,
                    cancellationToken);
            }
        }

        private async Task NotifyCustomerAboutCancellationAsync(
            Booking booking,
            BookingStatus previousStatus,
            string cancellationActor,
            string cancellationReason,
            CancellationToken cancellationToken)
        {
            var identityUser = await _identityUserReadClient.GetUserByIdAsync(booking.UserId, cancellationToken);
            if (identityUser is null || string.IsNullOrWhiteSpace(identityUser.Email))
            {
                return;
            }

            var clientProfile = await _clientBookingAccessClient.GetClientProfileAsync(booking.UserId, cancellationToken);
            var customerFullName = BuildPersonName(clientProfile?.FirstName, clientProfile?.LastName);
            if (string.IsNullOrWhiteSpace(customerFullName))
            {
                customerFullName = string.IsNullOrWhiteSpace(identityUser.Username)
                    ? "Клиент"
                    : identityUser.Username.Trim();
            }

            await _bookingEmailClient.SendCustomEmailAsync(
                identityUser.Email,
                $"Booking #{booking.Id}: бронирование отменено",
                BuildCancellationEmailText(
                    booking.Id,
                    customerFullName,
                    cancellationActor,
                    cancellationReason,
                    previousStatus),
                cancellationToken: cancellationToken);
        }

        private async Task NotifyPartnerAboutCancellationRequestAsync(
            string email,
            string partnerFullName,
            Booking booking,
            string partnerReason,
            CancellationToken cancellationToken)
        {
            try
            {
                await _bookingEmailClient.SendCustomEmailAsync(
                    email,
                    $"Booking #{booking.Id}: запрос на отмену отправлен",
                    BuildPartnerCancellationRequestEmailText(
                        booking.Id,
                        partnerFullName,
                        booking.CarBrand,
                        booking.CarModel,
                        booking.StartTime,
                        booking.EndTime,
                        partnerReason),
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send partner cancellation request email for booking {BookingId} to {Email}.",
                    booking.Id,
                    email);
            }
        }

        private static string BuildCancellationEmailText(
            int bookingId,
            string customerFullName,
            string cancellationActor,
            string cancellationReason,
            BookingStatus previousStatus)
        {
            var normalizedName = string.IsNullOrWhiteSpace(customerFullName) ? "Клиент" : customerFullName.Trim();
            var actorText = GetCancellationActorDisplayText(cancellationActor);
            var paymentText = previousStatus == BookingStatus.Pending
                ? "Оплата по бронированию не была завершена. Если попытка оплаты уже была начата, она аннулирована."
                : "Оплата по бронированию отменена, возврат средств оформлен.";

            return $"{normalizedName}, бронирование #{bookingId} было отменено {actorText}. " +
                   $"Причина отмены: {cancellationReason}. {paymentText}";
        }

        private static string BuildPartnerCancellationRequestEmailText(
            int bookingId,
            string partnerFullName,
            string? carBrand,
            string? carModel,
            DateTimeOffset bookingStartTime,
            DateTimeOffset bookingEndTime,
            string partnerReason)
        {
            var normalizedName = string.IsNullOrWhiteSpace(partnerFullName) ? "Партнер" : partnerFullName.Trim();
            var bookingTitle = BuildBookingTitle(carBrand, carModel);
            var bookingWindow =
                $"{bookingStartTime:dd.MM.yyyy HH:mm} - {bookingEndTime:dd.MM.yyyy HH:mm}";

            return $"{normalizedName}, ваш запрос на отмену бронирования #{bookingId} отправлен менеджеру на рассмотрение. " +
                   $"Автомобиль: {bookingTitle}. Период бронирования: {bookingWindow}. " +
                   $"Указанная причина: {partnerReason}. Мы пришлем отдельное уведомление после решения менеджера.";
        }

        private static string BuildBookingTitle(string? carBrand, string? carModel)
        {
            var title = string.Join(
                " ",
                new[] { carBrand?.Trim(), carModel?.Trim() }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));

            return string.IsNullOrWhiteSpace(title) ? "не указан" : title;
        }

        private static string NormalizeCancellationActor(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                CancellationActorClient => CancellationActorClient,
                CancellationActorPartner => CancellationActorPartner,
                CancellationActorManager => CancellationActorManager,
                _ => CancellationActorManager
            };
        }

        private static string NormalizeCancellationReason(string? value, string cancellationActor)
        {
            var normalized = NormalizeOptionalText(value, nameof(value), 2000);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }

            return cancellationActor switch
            {
                CancellationActorClient => "Бронирование отменено клиентом.",
                CancellationActorPartner => "Бронирование отменено партнером.",
                _ => "Бронирование отменено менеджером."
            };
        }

        private static string GetCancellationActorDisplayText(string? cancellationActor)
        {
            return NormalizeCancellationActor(cancellationActor) switch
            {
                CancellationActorClient => "клиентом",
                CancellationActorPartner => "партнером",
                _ => "менеджером"
            };
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
                BookingStatus.Active => targetStatus is BookingStatus.AwaitingReview or BookingStatus.Completed or BookingStatus.Canceled,
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

        private async Task EnsureMockPaymentSessionStartedAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            if (booking.Status != BookingStatus.Pending)
            {
                return;
            }

            var totalPrice = ResolveBookingPaymentAmount(booking);
            var currency = ResolvePaymentCurrency();

            try
            {
                await _paymentSyncClient.StartMockPaymentAsync(
                    booking.Id,
                    booking.UserId,
                    totalPrice,
                    currency,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to eagerly start mock payment session for booking {BookingId}. Payment session will be retried on checkout open.",
                    booking.Id);
            }
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

        private sealed record BookingDisplaySnapshot(
            string CarBrand,
            string CarModel,
            string PartnerName,
            string? CoverImageUrl,
            IReadOnlyList<string> ImageUrls);
    }
}
