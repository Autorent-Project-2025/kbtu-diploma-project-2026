using TicketService.Domain.Entities;

namespace TicketService.Application.Interfaces;

public interface IReopenRequestRepository
{
    Task AddAsync(ComplaintReopenRequest request, CancellationToken cancellationToken = default);

    Task<ComplaintReopenRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ComplaintReopenRequest>> GetByComplaintIdAsync(
        Guid complaintId, CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingForComplaintAsync(
        Guid complaintId, CancellationToken cancellationToken = default);
}
