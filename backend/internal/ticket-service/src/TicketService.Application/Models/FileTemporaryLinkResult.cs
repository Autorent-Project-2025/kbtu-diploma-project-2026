namespace TicketService.Application.Models;

public sealed record FileTemporaryLinkResult(
    string FileName,
    string Url,
    DateTime ExpiresAtUtc);
