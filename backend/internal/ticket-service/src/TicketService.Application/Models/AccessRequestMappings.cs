using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public static class AccessRequestMappings
{
    public static AccessRequestDto ToDto(this ComplaintBookingAccessRequest request)
    {
        return new AccessRequestDto(
            request.Id,
            request.ComplaintId,
            request.BookingId,
            request.RequestedByManagerId,
            request.Status,
            request.Reason,
            request.RequestedAt,
            request.ReviewedBySupermanagerId,
            request.ReviewedAt,
            request.DecisionNote,
            request.ExpiresAt);
    }
}
