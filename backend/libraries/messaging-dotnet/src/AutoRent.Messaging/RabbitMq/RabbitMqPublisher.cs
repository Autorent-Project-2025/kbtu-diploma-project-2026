using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AutoRent.Messaging.RabbitMq;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly ConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _connectionFactory = RabbitMqConnectionFactoryBuilder.Build(_options);
    }

    public async Task PublishAsync<TPayload>(
        string eventId,
        string routingKey,
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("Event id is required.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException("Routing key is required.", nameof(routingKey));
        }

        var connection = await GetConnectionAsync(cancellationToken);
        using var channel = connection.CreateModel();

        RabbitMqTopology.DeclareExchange(channel, _options);

        var message = new IntegrationMessage<TPayload>(
            eventId.Trim(),
            routingKey.Trim(),
            DateTimeOffset.UtcNow,
            payload);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = message.EventId;
        properties.Type = message.RoutingKey;
        properties.ContentType = "application/json";

        var body = RabbitMqJson.Serialize(message);
        channel.BasicPublish(
            exchange: RabbitMqTopology.ResolveExchangeName(_options),
            routingKey: message.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Published RabbitMQ message {EventId} with routing key {RoutingKey}.",
            message.EventId,
            message.RoutingKey);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection?.Dispose();
        _connectionLock.Dispose();
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = _connectionFactory.CreateConnection();
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

}
