using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;

namespace TicketService.Application.Complaints.Queries.GetComplaintActionLogs;

public sealed record GetComplaintActionLogsQuery(Guid ComplaintId);

public sealed record ComplaintActionLogDto(
    Guid Id,
    Guid ComplaintId,
    string ActionType,
    Guid PerformedBy,
    string? Comment,
    string? TargetEntityType,
    string? TargetEntityId,
    DateTime CreatedAt);

public sealed record GetComplaintActionLogsResult(IReadOnlyCollection<ComplaintActionLogDto> ActionLogs);

public sealed class GetComplaintActionLogsQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintActionLogsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetComplaintActionLogsResult> Handle(
        GetComplaintActionLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");

        var logs = await _complaintRepository.GetActionLogsAsync(query.ComplaintId, cancellationToken);

        var dtos = logs.Select(l => new ComplaintActionLogDto(
            l.Id, l.ComplaintId, l.ActionType, l.PerformedBy,
            l.Comment, l.TargetEntityType, l.TargetEntityId, l.CreatedAt)).ToArray();

        return new GetComplaintActionLogsResult(dtos);
    }
}
