using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequestForComplaint;

public sealed record GetAccessRequestForComplaintResult(AccessRequestDto? AccessRequest);
