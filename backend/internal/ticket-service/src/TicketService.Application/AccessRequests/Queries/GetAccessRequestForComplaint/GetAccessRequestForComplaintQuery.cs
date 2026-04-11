namespace TicketService.Application.AccessRequests.Queries.GetAccessRequestForComplaint;

public sealed record GetAccessRequestForComplaintQuery(Guid ComplaintId, Guid ManagerId);
