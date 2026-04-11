using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetComplaintById;

public sealed class GetComplaintByIdQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintByIdQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetComplaintByIdResult> Handle(
        GetComplaintByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(query.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{query.ComplaintId}' was not found.");

        return new GetComplaintByIdResult(complaint.ToDto());
    }
}
