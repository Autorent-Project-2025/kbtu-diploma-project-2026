using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.Api.Contracts.AccessRequests;
using TicketService.Application.AccessRequests.Commands.ApproveAccessRequest;
using TicketService.Application.AccessRequests.Commands.CreateAccessRequest;
using TicketService.Application.AccessRequests.Commands.RejectAccessRequest;
using TicketService.Application.AccessRequests.Commands.RevokeAccessRequest;
using TicketService.Application.AccessRequests.Queries.GetAccessRequestById;
using TicketService.Application.AccessRequests.Queries.GetAccessRequestForComplaint;
using TicketService.Application.AccessRequests.Queries.GetAccessRequests;
using TicketService.Application.AccessRequests.Queries.GetBookingReview;
using TicketService.Application.Constants;
using TicketService.Application.Exceptions;
using TicketService.Domain.Enums;

namespace TicketService.Api.Controllers;

[ApiController]
[Route("complaints")]
public sealed class BookingAccessController : ControllerBase
{
    private readonly CreateAccessRequestCommandHandler _createHandler;
    private readonly ApproveAccessRequestCommandHandler _approveHandler;
    private readonly RejectAccessRequestCommandHandler _rejectHandler;
    private readonly RevokeAccessRequestCommandHandler _revokeHandler;
    private readonly GetAccessRequestsQueryHandler _getAllHandler;
    private readonly GetAccessRequestByIdQueryHandler _getByIdHandler;
    private readonly GetAccessRequestForComplaintQueryHandler _getForComplaintHandler;
    private readonly GetBookingReviewQueryHandler _bookingReviewHandler;

    public BookingAccessController(
        CreateAccessRequestCommandHandler createHandler,
        ApproveAccessRequestCommandHandler approveHandler,
        RejectAccessRequestCommandHandler rejectHandler,
        RevokeAccessRequestCommandHandler revokeHandler,
        GetAccessRequestsQueryHandler getAllHandler,
        GetAccessRequestByIdQueryHandler getByIdHandler,
        GetAccessRequestForComplaintQueryHandler getForComplaintHandler,
        GetBookingReviewQueryHandler bookingReviewHandler)
    {
        _createHandler = createHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _revokeHandler = revokeHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getForComplaintHandler = getForComplaintHandler;
        _bookingReviewHandler = bookingReviewHandler;
    }

    // ── Manager endpoints ──

    /// <summary>
    /// Create a booking access request for the complaint the manager is reviewing.
    /// Requires: Complaint.Review permission + manager must be assigned to the complaint.
    /// </summary>
    [Authorize(Policy = "complaints:review")]
    [HttpPost("{complaintId:guid}/booking-access-requests")]
    public async Task<IActionResult> CreateAccessRequest(
        Guid complaintId,
        [FromBody] CreateAccessRequestRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();

        var result = await _createHandler.Handle(
            new CreateAccessRequestCommand(complaintId, managerId, request.Reason),
            cancellationToken);

        return Created(
            $"/complaints/access-requests/{result.AccessRequest.Id}",
            result.AccessRequest);
    }

    /// <summary>
    /// Get access request status for a specific complaint (for the current manager).
    /// </summary>
    [Authorize(Policy = "complaints:review")]
    [HttpGet("{complaintId:guid}/booking-access-requests/mine")]
    public async Task<IActionResult> GetMyAccessRequest(
        Guid complaintId,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();

        var result = await _getForComplaintHandler.Handle(
            new GetAccessRequestForComplaintQuery(complaintId, managerId),
            cancellationToken);

        return Ok(result.AccessRequest);
    }

    /// <summary>
    /// Read-only booking review, accessible if the manager has Booking.View
    /// OR has an active complaint-scoped grant.
    /// </summary>
    [Authorize(Policy = "complaints:review")]
    [HttpGet("{complaintId:guid}/booking-review")]
    public async Task<IActionResult> GetBookingReview(
        Guid complaintId,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var hasGlobalBookingView = HasPermission(PermissionConstants.BookingView);

        var result = await _bookingReviewHandler.Handle(
            new GetBookingReviewQuery(complaintId, managerId, hasGlobalBookingView),
            cancellationToken);

        return Ok(result.Booking);
    }

    // ── Supermanager endpoints ──

    /// <summary>
    /// List all booking access requests (filterable by status).
    /// </summary>
    [Authorize(Policy = "access-requests:review")]
    [HttpGet("access-requests")]
    public async Task<IActionResult> GetAllAccessRequests(
        [FromQuery] int? status,
        CancellationToken cancellationToken)
    {
        var result = await _getAllHandler.Handle(
            new GetAccessRequestsQuery(
                status.HasValue ? (AccessRequestStatus)status.Value : null),
            cancellationToken);

        return Ok(result.AccessRequests);
    }

    /// <summary>
    /// Get a single access request by ID.
    /// </summary>
    [Authorize(Policy = "access-requests:review")]
    [HttpGet("access-requests/{id:guid}")]
    public async Task<IActionResult> GetAccessRequestById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(
            new GetAccessRequestByIdQuery(id),
            cancellationToken);

        return Ok(result.AccessRequest);
    }

    /// <summary>
    /// Approve a pending access request with expiry duration.
    /// </summary>
    [Authorize(Policy = "access-requests:review")]
    [HttpPost("access-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveAccessRequest(
        Guid id,
        [FromBody] ApproveAccessRequestRequest request,
        CancellationToken cancellationToken)
    {
        var supermanagerId = ResolveUserId();

        var result = await _approveHandler.Handle(
            new ApproveAccessRequestCommand(id, supermanagerId, request.DecisionNote, request.ExpiresInHours),
            cancellationToken);

        return Ok(result.AccessRequest);
    }

    /// <summary>
    /// Reject a pending access request.
    /// </summary>
    [Authorize(Policy = "access-requests:review")]
    [HttpPost("access-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectAccessRequest(
        Guid id,
        [FromBody] RejectAccessRequestRequest request,
        CancellationToken cancellationToken)
    {
        var supermanagerId = ResolveUserId();

        var result = await _rejectHandler.Handle(
            new RejectAccessRequestCommand(id, supermanagerId, request.DecisionNote),
            cancellationToken);

        return Ok(result.AccessRequest);
    }

    /// <summary>
    /// Revoke an approved access grant immediately.
    /// </summary>
    [Authorize(Policy = "access-requests:review")]
    [HttpPost("access-requests/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeAccessRequest(
        Guid id,
        CancellationToken cancellationToken)
    {
        var supermanagerId = ResolveUserId();

        var result = await _revokeHandler.Handle(
            new RevokeAccessRequestCommand(id, supermanagerId),
            cancellationToken);

        return Ok(result.AccessRequest);
    }

    // ── Helpers ──

    private Guid ResolveUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var parsed))
            throw new UnauthorizedException("Authenticated user id claim is required.");
        return parsed;
    }

    private bool HasPermission(string permission)
    {
        return User.Claims.Any(c =>
            c.Type == "permissions" &&
            c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
    }
}
