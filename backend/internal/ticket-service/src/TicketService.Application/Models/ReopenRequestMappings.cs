using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public static class ReopenRequestMappings
{
    public static ReopenRequestDto ToDto(this ComplaintReopenRequest request)
    {
        return new ReopenRequestDto(
            request.Id,
            request.ComplaintId,
            request.RequestedByUserId,
            request.Reason,
            request.Status,
            request.ReviewedByManagerId,
            request.ReviewedAt,
            request.DecisionNote,
            request.CreatedAt);
    }
}
