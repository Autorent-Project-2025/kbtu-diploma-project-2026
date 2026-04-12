using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Services;

/// <summary>
/// Ensures old complaints (created before chat integration) have a conversation.
/// Creates conversation and seeds messages from legacy fields on first access.
/// </summary>
public sealed class ComplaintChatMigrationService
{
    private readonly IChatServiceClient _chatServiceClient;

    public ComplaintChatMigrationService(IChatServiceClient chatServiceClient)
    {
        _chatServiceClient = chatServiceClient;
    }

    /// <summary>
    /// Ensures the complaint has a conversation. If not, creates one and seeds
    /// messages from legacy complaint fields. Fire-and-forget safe.
    /// </summary>
    public async Task EnsureConversationExistsAsync(Complaint complaint, CancellationToken ct = default)
    {
        try
        {
            var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
                "complaint", complaint.Id.ToString(), ct);

            if (conversationId is not null)
                return;

            // Determine reporter actor type
            var actorType = complaint.ReporterActorType == ReporterActorType.Client
                ? "client" : "partner";

            var participants = new List<ChatParticipant>
            {
                new(complaint.CreatedByUserId.ToString(), actorType, "reporter",
                    CanRead: true, CanWrite: true, CanSendInternal: false)
            };

            // If manager was assigned, include them
            if (complaint.AssignedToManagerId.HasValue)
            {
                participants.Add(new ChatParticipant(
                    complaint.AssignedToManagerId.Value.ToString(),
                    "manager", "manager",
                    CanRead: true,
                    CanWrite: !IsTerminal(complaint.Status),
                    CanSendInternal: true));
            }

            conversationId = await _chatServiceClient.CreateConversationAsync(
                "complaint",
                complaint.Id.ToString(),
                "ticket-service",
                participants,
                "Жалоба создана (миграция из старой системы)",
                ct);

            if (conversationId is null)
                return;

            // Seed messages from legacy fields in chronological order
            await SeedLegacyMessagesAsync(conversationId, complaint, ct);

            // If complaint is resolved/rejected, close the conversation
            if (IsTerminal(complaint.Status))
            {
                var closeMessage = complaint.Status == ComplaintStatus.Resolved
                    ? "Жалоба решена" : "Жалоба отклонена";
                await _chatServiceClient.CloseConversationAsync(conversationId, closeMessage, ct);
            }
        }
        catch
        {
            // Non-blocking: migration failure should not break complaint viewing
        }
    }

    private async Task SeedLegacyMessagesAsync(string conversationId, Complaint complaint, CancellationToken ct)
    {
        // Seed description as initial message
        if (!string.IsNullOrWhiteSpace(complaint.Description))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Описание жалобы: {complaint.Description}",
                internalOnly: false, ct);
        }

        // Seed info request
        if (!string.IsNullOrWhiteSpace(complaint.InfoRequestText))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Запрос информации от менеджера: {complaint.InfoRequestText}",
                internalOnly: false, ct);
        }

        // Seed info response
        if (!string.IsNullOrWhiteSpace(complaint.InfoResponseText))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Ответ заявителя: {complaint.InfoResponseText}",
                internalOnly: false, ct);
        }

        // Seed manager note (internal only)
        if (!string.IsNullOrWhiteSpace(complaint.ManagerNote))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Заметка менеджера: {complaint.ManagerNote}",
                internalOnly: true, ct);
        }

        // Seed resolution note
        if (!string.IsNullOrWhiteSpace(complaint.ResolutionNote))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Решение: {complaint.ResolutionNote}",
                internalOnly: false, ct);
        }

        // Seed rejection reason
        if (!string.IsNullOrWhiteSpace(complaint.RejectionReason))
        {
            await _chatServiceClient.SendSystemMessageAsync(
                conversationId,
                $"Причина отклонения: {complaint.RejectionReason}",
                internalOnly: false, ct);
        }
    }

    private static bool IsTerminal(ComplaintStatus status) =>
        status is ComplaintStatus.Resolved or ComplaintStatus.Rejected;
}
