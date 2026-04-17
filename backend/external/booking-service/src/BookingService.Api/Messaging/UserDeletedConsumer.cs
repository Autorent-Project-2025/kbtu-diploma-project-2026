using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using BookingService.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BookingService.Api.Messaging;

public sealed class UserDeletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserDeletedConsumer> _logger;
    private readonly RabbitMqOptions _options;

    public UserDeletedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<UserDeletedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionFactory = RabbitMqConnectionFactoryBuilder.Build(_options, dispatchConsumersAsync: true);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                RabbitMqTopology.DeclareBoundQueue(
                    channel,
                    _options,
                    RabbitMqTopology.BookingUserDeletedQueue,
                    RabbitMqTopology.RoutingKeys.UserDeleted);

                channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, args) => await HandleMessageAsync(channel, args, stoppingToken);

                var consumerTag = channel.BasicConsume(
                    queue: RabbitMqTopology.BookingUserDeletedQueue,
                    autoAck: false,
                    consumer: consumer);

                try
                {
                    while (!stoppingToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                }
                finally
                {
                    if (channel.IsOpen)
                    {
                        channel.BasicCancel(consumerTag);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserDeleted RabbitMQ consumer failed. Restarting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var message = RabbitMqJson.Deserialize<UserDeleted>(args.Body)
                ?? throw new InvalidOperationException("UserDeleted message is invalid.");

            _logger.LogInformation(
                "Received user.deleted event for UserId={UserId}. Canceling active bookings.",
                message.Payload.UserId);

            using var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            var canceledCount = await bookingService.CancelActiveBookingsByUserAsync(
                message.Payload.UserId,
                "Бронирование отменено в связи с удалением аккаунта.",
                cancellationToken);

            _logger.LogInformation(
                "Canceled {Count} active bookings for deleted user {UserId}.",
                canceledCount, message.Payload.UserId);

            channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "UserDeleted consumer failed. Message will be requeued.");
            channel.BasicNack(args.DeliveryTag, false, true);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
