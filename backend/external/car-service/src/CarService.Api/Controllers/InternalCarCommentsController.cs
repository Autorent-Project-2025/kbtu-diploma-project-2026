using System.Security.Cryptography;
using System.Text;
using CarService.Api.Contracts.Internal;
using CarService.Api.Options;
using CarService.Application.DTOs.CarComment;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CarService.Api.Controllers;

[ApiController]
[Route("internal/comments")]
public sealed class InternalCarCommentsController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly ICarCommentService _carCommentService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalCarCommentsController(
        ICarCommentService carCommentService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _carCommentService = carCommentService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCarCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var created = await _carCommentService.CreateFromCompletedBookingAsync(
            new CarCommentCreateDto
            {
                BookingId = request.BookingId,
                PartnerCarId = request.PartnerCarId,
                Rating = request.Rating,
                Content = request.Content
            },
            request.UserId,
            request.UserName,
            cancellationToken);

        return Ok(created);
    }

    private bool IsAuthorizedInternalRequest()
    {
        if (string.IsNullOrWhiteSpace(_internalAuthOptions.ApiKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue(InternalApiKeyHeader, out var receivedApiKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(_internalAuthOptions.ApiKey);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedApiKey.ToString());

        return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }
}
