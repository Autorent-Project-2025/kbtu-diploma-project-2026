using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.Api.Contracts.Tickets;
using TicketService.Application.Commands.ApproveTicket;
using TicketService.Application.Commands.CreateTicket;
using TicketService.Application.Commands.RejectTicket;
using TicketService.Application.Exceptions;
using TicketService.Application.Queries.GetPendingTickets;
using TicketService.Application.Queries.GetTicketById;

namespace TicketService.Api.Controllers;

[ApiController]
[Route("")]
public sealed class TicketsController : ControllerBase
{
    private readonly CreateTicketCommandHandler _createTicketCommandHandler;
    private readonly GetPendingTicketsQueryHandler _getPendingTicketsQueryHandler;
    private readonly GetTicketByIdQueryHandler _getTicketByIdQueryHandler;
    private readonly ApproveTicketCommandHandler _approveTicketCommandHandler;
    private readonly RejectTicketCommandHandler _rejectTicketCommandHandler;

    public TicketsController(
        CreateTicketCommandHandler createTicketCommandHandler,
        GetPendingTicketsQueryHandler getPendingTicketsQueryHandler,
        GetTicketByIdQueryHandler getTicketByIdQueryHandler,
        ApproveTicketCommandHandler approveTicketCommandHandler,
        RejectTicketCommandHandler rejectTicketCommandHandler)
    {
        _createTicketCommandHandler = createTicketCommandHandler;
        _getPendingTicketsQueryHandler = getPendingTicketsQueryHandler;
        _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
        _approveTicketCommandHandler = approveTicketCommandHandler;
        _rejectTicketCommandHandler = rejectTicketCommandHandler;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createTicketCommandHandler.Handle(
            new CreateTicketCommand(request.FullName, request.Email, request.BirthDate),
            cancellationToken);

        return Created($"/{result.Ticket.Id}", result.Ticket);
    }

    [Authorize(Policy = "tickets:view")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await _getPendingTicketsQueryHandler.Handle(new GetPendingTicketsQuery(), cancellationToken);
        return Ok(result.Tickets);
    }

    [Authorize(Policy = "tickets:view")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _getTicketByIdQueryHandler.Handle(new GetTicketByIdQuery(id), cancellationToken);
        return Ok(result.Ticket);
    }

    [Authorize(Policy = "tickets:approve")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var managerId = ResolveManagerId();
        var result = await _approveTicketCommandHandler.Handle(
            new ApproveTicketCommand(id, managerId),
            cancellationToken);

        return Ok(result.Ticket);
    }

    [Authorize(Policy = "tickets:reject")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id,
        [FromBody] RejectTicketRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveManagerId();
        var result = await _rejectTicketCommandHandler.Handle(
            new RejectTicketCommand(id, managerId, request.DecisionReason),
            cancellationToken);

        return Ok(result.Ticket);
    }

    private Guid ResolveManagerId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var managerId))
        {
            throw new UnauthorizedException("Authenticated manager id claim is required.");
        }

        return managerId;
    }
}
