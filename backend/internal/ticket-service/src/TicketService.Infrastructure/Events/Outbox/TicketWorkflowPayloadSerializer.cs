using System.Text.Json;
using System.Text.Json.Serialization;

namespace TicketService.Infrastructure.Events.Outbox;

internal static class TicketWorkflowPayloadSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static string Serialize<TPayload>(TPayload payload)
    {
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    public static TPayload Deserialize<TPayload>(string payload)
    {
        var model = JsonSerializer.Deserialize<TPayload>(payload, SerializerOptions);
        if (model is null)
        {
            throw new InvalidOperationException("Ticket workflow payload is invalid.");
        }

        return model;
    }
}
