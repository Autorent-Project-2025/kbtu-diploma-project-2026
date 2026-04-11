using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.Api.Contracts.Complaints;
using TicketService.Application.Complaints.Commands.AddManagerNote;
using TicketService.Application.Complaints.Commands.CreateComplaint;
using TicketService.Application.Complaints.Commands.RejectComplaint;
using TicketService.Application.Complaints.Commands.RequestInfo;
using TicketService.Application.Complaints.Commands.RespondToInfoRequest;
using TicketService.Application.Complaints.Commands.ResolveComplaint;
using TicketService.Application.Complaints.Commands.TakeComplaint;
using TicketService.Application.Complaints.Queries.GetAllComplaints;
using TicketService.Application.Complaints.Queries.GetComplaintById;
using TicketService.Application.Complaints.Queries.GetMyComplaintById;
using TicketService.Application.Complaints.Queries.GetMyComplaints;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Enums;

namespace TicketService.Api.Controllers;

[ApiController]
[Route("complaints")]
public sealed class ComplaintsController : ControllerBase
{
    private readonly CreateComplaintCommandHandler _createComplaintHandler;
    private readonly TakeComplaintCommandHandler _takeComplaintHandler;
    private readonly RequestInfoCommandHandler _requestInfoHandler;
    private readonly RespondToInfoRequestCommandHandler _respondToInfoRequestHandler;
    private readonly AddManagerNoteCommandHandler _addManagerNoteHandler;
    private readonly ResolveComplaintCommandHandler _resolveComplaintHandler;
    private readonly RejectComplaintCommandHandler _rejectComplaintHandler;
    private readonly GetMyComplaintsQueryHandler _getMyComplaintsHandler;
    private readonly GetMyComplaintByIdQueryHandler _getMyComplaintByIdHandler;
    private readonly GetAllComplaintsQueryHandler _getAllComplaintsHandler;
    private readonly GetComplaintByIdQueryHandler _getComplaintByIdHandler;
    private readonly IFileStorageClient _fileStorageClient;

    public ComplaintsController(
        CreateComplaintCommandHandler createComplaintHandler,
        TakeComplaintCommandHandler takeComplaintHandler,
        RequestInfoCommandHandler requestInfoHandler,
        RespondToInfoRequestCommandHandler respondToInfoRequestHandler,
        AddManagerNoteCommandHandler addManagerNoteHandler,
        ResolveComplaintCommandHandler resolveComplaintHandler,
        RejectComplaintCommandHandler rejectComplaintHandler,
        GetMyComplaintsQueryHandler getMyComplaintsHandler,
        GetMyComplaintByIdQueryHandler getMyComplaintByIdHandler,
        GetAllComplaintsQueryHandler getAllComplaintsHandler,
        GetComplaintByIdQueryHandler getComplaintByIdHandler,
        IFileStorageClient fileStorageClient)
    {
        _createComplaintHandler = createComplaintHandler;
        _takeComplaintHandler = takeComplaintHandler;
        _requestInfoHandler = requestInfoHandler;
        _respondToInfoRequestHandler = respondToInfoRequestHandler;
        _addManagerNoteHandler = addManagerNoteHandler;
        _resolveComplaintHandler = resolveComplaintHandler;
        _rejectComplaintHandler = rejectComplaintHandler;
        _getMyComplaintsHandler = getMyComplaintsHandler;
        _getMyComplaintByIdHandler = getMyComplaintByIdHandler;
        _getAllComplaintsHandler = getAllComplaintsHandler;
        _getComplaintByIdHandler = getComplaintByIdHandler;
        _fileStorageClient = fileStorageClient;
    }

    // ── External endpoints (JWT + ownership check) ──

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateComplaintRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var actorType = ResolveActorType();
        var reporterActorType = ParseReporterActorType(actorType);
        var targetType = ParseTargetType(request.TargetType);
        var category = ParseCategory(request.Category);

        var attachments = await MapToFilePayloadsAsync(request.Attachments, cancellationToken);

        var result = await _createComplaintHandler.Handle(
            new CreateComplaintCommand(
                userId,
                reporterActorType,
                request.BookingId,
                targetType,
                category,
                request.Subject,
                request.Description,
                attachments),
            cancellationToken);

        return Created($"/complaints/my/{result.Complaint.Id}", result.Complaint);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var result = await _getMyComplaintsHandler.Handle(
            new GetMyComplaintsQuery(userId), cancellationToken);
        return Ok(result.Complaints);
    }

    [Authorize]
    [HttpGet("my/{id:guid}")]
    public async Task<IActionResult> GetMyById(Guid id, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var result = await _getMyComplaintByIdHandler.Handle(
            new GetMyComplaintByIdQuery(id, userId), cancellationToken);
        return Ok(result.Complaint);
    }

    [Authorize]
    [HttpPost("my/{id:guid}/respond")]
    public async Task<IActionResult> Respond(
        Guid id,
        [FromForm] RespondToInfoRequestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var attachments = await MapToFilePayloadsAsync(request.Attachments, cancellationToken);

        var result = await _respondToInfoRequestHandler.Handle(
            new RespondToInfoRequestCommand(id, userId, request.Message, attachments),
            cancellationToken);

        return Ok(result.Complaint);
    }

    [Authorize]
    [HttpGet("my/{id:guid}/attachments/{attachmentId:guid}/temporary-link")]
    public async Task<IActionResult> GetMyAttachmentLink(
        Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var complaintResult = await _getMyComplaintByIdHandler.Handle(
            new GetMyComplaintByIdQuery(id, userId), cancellationToken);

        var attachment = complaintResult.Complaint.Attachments
            .FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
            throw new NotFoundException("Attachment not found.");

        var link = await _fileStorageClient.GetTemporaryLinkAsync(
            attachment.FileName, cancellationToken: cancellationToken);
        return Ok(link);
    }

    // ── Internal endpoints (JWT + permission policies) ──

    [Authorize(Policy = "complaints:view")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? status,
        [FromQuery] int? category,
        [FromQuery] int? priority,
        [FromQuery] Guid? assignedTo,
        CancellationToken cancellationToken)
    {
        var result = await _getAllComplaintsHandler.Handle(
            new GetAllComplaintsQuery(
                status.HasValue ? (ComplaintStatus)status.Value : null,
                category.HasValue ? (ComplaintCategory)category.Value : null,
                priority.HasValue ? (ComplaintPriority)priority.Value : null,
                assignedTo),
            cancellationToken);

        return Ok(result.Complaints);
    }

    [Authorize(Policy = "complaints:view")]
    [HttpGet("all/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getComplaintByIdHandler.Handle(
            new GetComplaintByIdQuery(id), cancellationToken);
        return Ok(result.Complaint);
    }

    [Authorize(Policy = "complaints:view")]
    [HttpGet("all/{id:guid}/attachments/{attachmentId:guid}/temporary-link")]
    public async Task<IActionResult> GetAttachmentLink(
        Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var complaintResult = await _getComplaintByIdHandler.Handle(
            new GetComplaintByIdQuery(id), cancellationToken);

        var attachment = complaintResult.Complaint.Attachments
            .FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
            throw new NotFoundException("Attachment not found.");

        var link = await _fileStorageClient.GetTemporaryLinkAsync(
            attachment.FileName, cancellationToken: cancellationToken);
        return Ok(link);
    }

    [Authorize(Policy = "complaints:review")]
    [HttpPost("all/{id:guid}/take")]
    public async Task<IActionResult> Take(Guid id, CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var result = await _takeComplaintHandler.Handle(
            new TakeComplaintCommand(id, managerId), cancellationToken);
        return Ok(result.Complaint);
    }

    [Authorize(Policy = "complaints:review")]
    [HttpPost("all/{id:guid}/request-info")]
    public async Task<IActionResult> RequestInfo(
        Guid id,
        [FromBody] Contracts.Complaints.RequestInfoRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var result = await _requestInfoHandler.Handle(
            new RequestInfoCommand(id, managerId, request.Message), cancellationToken);
        return Ok(result.Complaint);
    }

    [Authorize(Policy = "complaints:review")]
    [HttpPost("all/{id:guid}/note")]
    public async Task<IActionResult> AddNote(
        Guid id,
        [FromBody] AddManagerNoteRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var result = await _addManagerNoteHandler.Handle(
            new AddManagerNoteCommand(id, managerId, request.Note), cancellationToken);
        return Ok(result.Complaint);
    }

    [Authorize(Policy = "complaints:resolve")]
    [HttpPost("all/{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveComplaintRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var resolutionType = ParseResolutionType(request.ResolutionType);

        var result = await _resolveComplaintHandler.Handle(
            new ResolveComplaintCommand(id, managerId, resolutionType, request.ResolutionNote),
            cancellationToken);

        return Ok(result.Complaint);
    }

    [Authorize(Policy = "complaints:resolve")]
    [HttpPost("all/{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectComplaintRequest request,
        CancellationToken cancellationToken)
    {
        var managerId = ResolveUserId();
        var result = await _rejectComplaintHandler.Handle(
            new RejectComplaintCommand(id, managerId, request.Reason), cancellationToken);
        return Ok(result.Complaint);
    }

    // ── Helpers ──

    private Guid ResolveUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var parsed))
            throw new UnauthorizedException("Authenticated user id claim is required.");
        return parsed;
    }

    private string ResolveActorType()
    {
        return User.FindFirstValue("actor_type") ?? string.Empty;
    }

    private static ReporterActorType ParseReporterActorType(string actorType)
    {
        return actorType.Trim().ToLowerInvariant() switch
        {
            "client" => ReporterActorType.Client,
            "partner" => ReporterActorType.Partner,
            _ => throw new ValidationException("Only clients and partners can file complaints.")
        };
    }

    private static ComplaintTargetType ParseTargetType(string? value)
    {
        return (value?.Trim().ToLowerInvariant()) switch
        {
            "partner" => ComplaintTargetType.Partner,
            "client" => ComplaintTargetType.Client,
            _ => throw new ValidationException("targetType must be 'partner' or 'client'.")
        };
    }

    private static ComplaintCategory ParseCategory(string? value)
    {
        return (value?.Trim().ToLowerInvariant()) switch
        {
            "carcondition" or "car-condition" or "car_condition" => ComplaintCategory.CarCondition,
            "latehandover" or "late-handover" or "late_handover" => ComplaintCategory.LateHandover,
            "servicequality" or "service-quality" or "service_quality" => ComplaintCategory.ServiceQuality,
            "safetyissue" or "safety-issue" or "safety_issue" => ComplaintCategory.SafetyIssue,
            "clientmisbehavior" or "client-misbehavior" or "client_misbehavior" => ComplaintCategory.ClientMisbehavior,
            "other" => ComplaintCategory.Other,
            _ => throw new ValidationException(
                "category must be one of: car_condition, late_handover, service_quality, safety_issue, client_misbehavior, other.")
        };
    }

    private static ComplaintResolutionType ParseResolutionType(string? value)
    {
        return (value?.Trim().ToLowerInvariant()) switch
        {
            "in_favor_of_reporter" or "infavorofreporter" => ComplaintResolutionType.InFavorOfReporter,
            "in_favor_of_counterparty" or "infavorofcounterparty" => ComplaintResolutionType.InFavorOfCounterparty,
            "compromise_reached" or "compromisereached" => ComplaintResolutionType.CompromiseReached,
            "no_action_required" or "noactionrequired" => ComplaintResolutionType.NoActionRequired,
            _ => throw new ValidationException(
                "resolutionType must be one of: in_favor_of_reporter, in_favor_of_counterparty, compromise_reached, no_action_required.")
        };
    }

    private static async Task<IReadOnlyCollection<TicketDocumentFilePayload>?> MapToFilePayloadsAsync(
        IReadOnlyCollection<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return null;

        var payloads = new List<TicketDocumentFilePayload>(files.Count);
        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            payloads.Add(new TicketDocumentFilePayload(
                file.FileName,
                file.ContentType,
                memoryStream.ToArray()));
        }

        return payloads;
    }
}
