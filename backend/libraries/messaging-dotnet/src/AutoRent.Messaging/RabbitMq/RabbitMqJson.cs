using System.Text;
using System.Text.Json;

namespace AutoRent.Messaging.RabbitMq;

public static class RabbitMqJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static ReadOnlyMemory<byte> Serialize<TPayload>(IntegrationMessage<TPayload> message)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, SerializerOptions));
    }

    public static IntegrationMessage<TPayload>? Deserialize<TPayload>(ReadOnlyMemory<byte> body)
    {
        return JsonSerializer.Deserialize<IntegrationMessage<TPayload>>(body.Span, SerializerOptions);
    }
}
