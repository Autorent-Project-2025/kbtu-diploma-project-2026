using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using TicketService.Api.Contracts.Tickets;
using TicketService.Application.Commands.ApproveTicket;
using TicketService.Application.Commands.CreateTicket;
using TicketService.Application.Commands.IssueTicketFine;
using TicketService.Application.Commands.RejectTicket;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Application.Queries.GetAllTickets;
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
    private readonly GetAllTicketsQueryHandler _getAllTicketsQueryHandler;
    private readonly GetTicketByIdQueryHandler _getTicketByIdQueryHandler;
    private readonly ApproveTicketCommandHandler _approveTicketCommandHandler;
    private readonly IssueTicketFineCommandHandler _issueTicketFineCommandHandler;
    private readonly RejectTicketCommandHandler _rejectTicketCommandHandler;
    private readonly IFileStorageClient _fileStorageClient;

    public TicketsController(
        CreateTicketCommandHandler createTicketCommandHandler,
        GetPendingTicketsQueryHandler getPendingTicketsQueryHandler,
        GetAllTicketsQueryHandler getAllTicketsQueryHandler,
        GetTicketByIdQueryHandler getTicketByIdQueryHandler,
        ApproveTicketCommandHandler approveTicketCommandHandler,
        IssueTicketFineCommandHandler issueTicketFineCommandHandler,
        RejectTicketCommandHandler rejectTicketCommandHandler,
        IFileStorageClient fileStorageClient)
    {
        _createTicketCommandHandler = createTicketCommandHandler;
        _getPendingTicketsQueryHandler = getPendingTicketsQueryHandler;
        _getAllTicketsQueryHandler = getAllTicketsQueryHandler;
        _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
        _approveTicketCommandHandler = approveTicketCommandHandler;
        _issueTicketFineCommandHandler = issueTicketFineCommandHandler;
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
                ResolveRequestEmail(request, ticketType),
                request.BirthDate,
                request.PhoneNumber ?? string.Empty,
                request.AvatarUrl,
                await MapToOptionalFilePayloadAsync(request.IdentityDocumentFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.DriverLicenseFile, cancellationToken),
                request.CarBrand,
                request.CarModel,
                request.CarYear,
                request.LicensePlate,
                request.Transmission,
                request.FuelType,
                request.Seats,
                request.Doors,
                request.BodyType,
                request.Horsepower,
                request.SelectedTags,
                await MapToOptionalFilePayloadAsync(request.OwnershipDocumentFile, cancellationToken),
                await MapToFilePayloadCollectionAsync(request.CarImageFiles, cancellationToken),
                request.CarImageTypes,
                request.RelatedPartnerUserId,
                request.BookingId,
                request.BookingStatus,
                request.BookingStartTime,
                request.BookingEndTime,
                request.PartnerReason,
                request.PlannedStartTime,
                request.PlannedEndTime,
                request.TripStartedAt,
                request.TripCompletedAt,
                request.LatePenaltyAmount,
                await MapToOptionalFilePayloadAsync(request.CompletionFrontPhotoFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.CompletionBackPhotoFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.CompletionSideLeftPhotoFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.CompletionSideRightPhotoFile, cancellationToken),
                await MapToOptionalFilePayloadAsync(request.CompletionInteriorPhotoFile, cancellationToken),
                request.DamageAssessment),
            cancellationToken);

        return Created($"/{result.Ticket.Id}", result.Ticket);
    }

    [Authorize(Policy = "tickets:view-all")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _getAllTicketsQueryHandler.Handle(new GetAllTicketsQuery(search), cancellationToken);
        return Ok(result.Tickets);
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

    [Authorize(Policy = "tickets:approve")]
    [HttpPost("{id:guid}/issue-fine")]
    public async Task<IActionResult> IssueFine(
        [FromRoute] Guid id,
        [FromBody] IssueTicketFineRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveManagerId();
        var result = await _issueTicketFineCommandHandler.Handle(
            new IssueTicketFineCommand(id, managerId, request.Amount, request.Comment),
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
            "bookingcompletion" or "booking-completion" or "booking_completion" => TicketType.BookingCompletion,
            "partnerbookingcancellation" or "partner-booking-cancellation" or "partner_booking_cancellation" => TicketType.PartnerBookingCancellation,
            _ => throw new ValidationException("ticketType must be 'Client', 'Partner', 'PartnerCar', 'BookingCompletion' or 'PartnerBookingCancellation'.")
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
            "front" or "completion-front" => ResolveCompletionPhotoFileName(ticket, "front"),
            "back" or "completion-back" => ResolveCompletionPhotoFileName(ticket, "back"),
            "side-left" or "side_left" or "completion-side-left" => ResolveCompletionPhotoFileName(ticket, "side_left"),
            "side-right" or "side_right" or "completion-side-right" => ResolveCompletionPhotoFileName(ticket, "side_right"),
            "interior" or "completion-interior" => ResolveCompletionPhotoFileName(ticket, "interior"),
            _ => throw new ValidationException("documentType must be 'identity', 'license', 'ownership', 'front', 'back', 'side_left', 'side_right' or 'interior'.")
        };
    }

    private static string? ResolveCompletionPhotoFileName(TicketDto ticket, string slot)
    {
        var photo = ticket.CompletionPhotos.FirstOrDefault(item =>
            string.Equals(item.Slot, slot, StringComparison.OrdinalIgnoreCase));
        return photo?.FileName;
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
            request.CarYear,
            request.LicensePlate,
            request.Transmission,
            request.FuelType,
            request.Seats,
            request.Doors,
            request.BodyType,
            request.Horsepower,
            request.ConfirmedTags);
    }

    private string ResolveRequestEmail(CreateTicketRequest request, TicketType ticketType)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            return request.Email;
        }

        if (ticketType != TicketType.PartnerCar)
        {
            return string.Empty;
        }

        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? string.Empty;
    }
}
