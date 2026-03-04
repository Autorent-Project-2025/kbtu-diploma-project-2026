using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using TicketService.Api.Contracts.Tickets;
using TicketService.Application.Commands.ApproveTicket;
using TicketService.Application.Commands.CreateTicket;
using TicketService.Application.Commands.RejectTicket;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Application.Queries.GetPendingTickets;
using TicketService.Application.Queries.GetTicketById;
using TicketService.Domain.Enums;

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
    private readonly IFileStorageClient _fileStorageClient;

    public TicketsController(
        CreateTicketCommandHandler createTicketCommandHandler,
        GetPendingTicketsQueryHandler getPendingTicketsQueryHandler,
        GetTicketByIdQueryHandler getTicketByIdQueryHandler,
        ApproveTicketCommandHandler approveTicketCommandHandler,
        RejectTicketCommandHandler rejectTicketCommandHandler,
        IFileStorageClient fileStorageClient)
    {
        _createTicketCommandHandler = createTicketCommandHandler;
        _getPendingTicketsQueryHandler = getPendingTicketsQueryHandler;
        _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
        _approveTicketCommandHandler = approveTicketCommandHandler;
        _rejectTicketCommandHandler = rejectTicketCommandHandler;
        _fileStorageClient = fileStorageClient;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticketType = ResolveTicketType(request.TicketType);

        var result = await _createTicketCommandHandler.Handle(
            new CreateTicketCommand(
                Request.Headers.Authorization.ToString(),
                ticketType,
                request.FirstName ?? string.Empty,
                request.LastName ?? string.Empty,
                request.CompanyName,
                request.ContactEmail,
                request.Email ?? string.Empty,
                request.BirthDate,
                request.PhoneNumber ?? string.Empty,
                request.AvatarUrl,
                await MapToOptionalFilePayloadAsync(request.IdentityDocumentFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.DriverLicenseFile, cancellationToken),
                request.CarBrand,
                request.CarModel,
                request.LicensePlate,
                await MapToOptionalFilePayloadAsync(request.OwnershipDocumentFile, cancellationToken),
                await MapToFilePayloadCollectionAsync(request.CarImageFiles, cancellationToken)),
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

    [Authorize(Policy = "tickets:view")]
    [HttpGet("{id:guid}/documents/{documentType}/temporary-link")]
    public async Task<IActionResult> GetDocumentTemporaryLink(
        [FromRoute] Guid id,
        [FromRoute] string documentType,
        CancellationToken cancellationToken)
    {
        var result = await _getTicketByIdQueryHandler.Handle(new GetTicketByIdQuery(id), cancellationToken);

        var fileName = ResolveDocumentFileName(result.Ticket, documentType);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new NotFoundException("Document file was not found for this ticket.");
        }

        var temporaryLink = await _fileStorageClient.GetTemporaryLinkAsync(fileName, cancellationToken: cancellationToken);
        return Ok(temporaryLink);
    }

    [Authorize(Policy = "tickets:approve")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ApproveTicketRequest? request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveManagerId();
        var result = await _approveTicketCommandHandler.Handle(
            new ApproveTicketCommand(id, managerId, MapPartnerCarReviewData(request?.PartnerCarData)),
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
            new RejectTicketCommand(
                id,
                managerId,
                request.DecisionReason,
                MapPartnerCarReviewData(request.PartnerCarData)),
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

    private static async Task<TicketDocumentFilePayload> MapToFilePayloadAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        return new TicketDocumentFilePayload(
            file.FileName,
            file.ContentType,
            memoryStream.ToArray());
    }

    private static async Task<TicketDocumentFilePayload?> MapToOptionalFilePayloadAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        return await MapToFilePayloadAsync(file, cancellationToken);
    }

    private static async Task<IReadOnlyCollection<TicketDocumentFilePayload>?> MapToFilePayloadCollectionAsync(
        IReadOnlyCollection<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
        {
            return null;
        }

        var payloads = new List<TicketDocumentFilePayload>(files.Count);
        foreach (var file in files)
        {
            payloads.Add(await MapToFilePayloadAsync(file, cancellationToken));
        }

        return payloads;
    }

    private static TicketType ResolveTicketType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TicketType.Client;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "client" => TicketType.Client,
            "partner" => TicketType.Partner,
            "partnercar" or "partner-car" or "partner_car" => TicketType.PartnerCar,
            _ => throw new ValidationException("ticketType must be 'Client', 'Partner' or 'PartnerCar'.")
        };
    }

    private static string? ResolveDocumentFileName(TicketDto ticket, string documentType)
    {
        var normalized = documentType?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "identity" or "identity-document" or "id" => ticket.IdentityDocumentFileName,
            "license" or "driver-license" => ticket.DriverLicenseFileName,
            "ownership" or "ownership-document" => ticket.OwnershipDocumentFileName,
            _ => throw new ValidationException("documentType must be 'identity', 'license' or 'ownership'.")
        };
    }

    private static PartnerCarTicketReviewData? MapPartnerCarReviewData(PartnerCarTicketReviewDataRequest? request)
    {
        if (request is null)
        {
            return null;
        }

        return new PartnerCarTicketReviewData(
            request.CarBrand,
            request.CarModel,
            request.LicensePlate,
            request.Email);
    }
}
