using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IBookingCompletionWorkflowClient
{
    Task ApproveCompletionReviewAsync(
        ApproveBookingCompletionReviewWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task IssueCompletionFineAsync(
        IssueBookingCompletionFineWorkflowRequest request,
        CancellationToken cancellationToken = default);
}
