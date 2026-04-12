using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Interfaces;

public interface IAccessRequestRepository
{
    Task AddAsync(ComplaintBookingAccessRequest request, CancellationToken cancellationToken = default);

    Task<ComplaintBookingAccessRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingAsync(
        Guid complaintId,
        int bookingId,
        Guid managerId,
        CancellationToken cancellationToken = default);

    Task<ComplaintBookingAccessRequest?> GetActiveGrantAsync(
        Guid managerId,
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<ComplaintBookingAccessRequest?> GetForComplaintAndManagerAsync(
        Guid complaintId,
        Guid managerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ComplaintBookingAccessRequest>> GetAllFilteredAsync(
        AccessRequestStatus? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ComplaintBookingAccessRequest>> GetActiveGrantsByComplaintAsync(
        Guid complaintId,
        CancellationToken cancellationToken = default);
}
