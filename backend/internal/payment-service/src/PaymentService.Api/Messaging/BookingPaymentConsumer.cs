using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using Microsoft.Extensions.Options;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentService.Api.Messaging;

public sealed class BookingPaymentConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingPaymentConsumer> _logger;
    private readonly RabbitMqOptions _options;

    public BookingPaymentConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<BookingPaymentConsumer> logger)
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
                    RabbitMqTopology.BookingPaymentsQueue,
                    RabbitMqTopology.RoutingKeys.BookingPaymentConfirmed,
                    RabbitMqTopology.RoutingKeys.BookingPaymentCanceled,
                    RabbitMqTopology.RoutingKeys.BookingPaymentCompleted);

                channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, args) => await HandleMessageAsync(channel, args, stoppingToken);

                var consumerTag = channel.BasicConsume(
                    queue: RabbitMqTopology.BookingPaymentsQueue,
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
                _logger.LogError(ex, "Payment RabbitMQ consumer failed. Restarting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var paymentLedgerService = scope.ServiceProvider.GetRequiredService<IPaymentLedgerService>();

            switch (args.RoutingKey)
            {
                case RabbitMqTopology.RoutingKeys.BookingPaymentConfirmed:
                {
                    var message = RabbitMqJson.Deserialize<BookingPaymentConfirmed>(args.Body)
                        ?? throw new InvalidOperationException("Booking payment confirmed message is invalid.");

                    await paymentLedgerService.RecordBookingConfirmedAsync(
                        new BookingPaymentSnapshot(
                            message.Payload.BookingId,
                            message.Payload.UserId,
                            message.Payload.PartnerUserId,
                            message.Payload.PartnerCarId,
                            message.Payload.PriceHour,
                            message.Payload.TotalPrice),
                        message.EventId,
                        message.RoutingKey,
                        cancellationToken);
                    break;
                }

                case RabbitMqTopology.RoutingKeys.BookingPaymentCanceled:
                {
                    var message = RabbitMqJson.Deserialize<BookingPaymentCanceled>(args.Body)
                        ?? throw new InvalidOperationException("Booking payment canceled message is invalid.");

                    await paymentLedgerService.RecordBookingCanceledAsync(
                        message.Payload.BookingId,
                        message.EventId,
                        message.RoutingKey,
                        cancellationToken);
                    break;
                }

                case RabbitMqTopology.RoutingKeys.BookingPaymentCompleted:
                {
                    var message = RabbitMqJson.Deserialize<BookingPaymentCompleted>(args.Body)
                        ?? throw new InvalidOperationException("Booking payment completed message is invalid.");

                    await paymentLedgerService.RecordBookingCompletedAsync(
                        message.Payload.BookingId,
                        message.EventId,
                        message.RoutingKey,
                        cancellationToken);
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unsupported booking payment routing key '{args.RoutingKey}'.");
            }

            channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Payment consumer failed for routing key {RoutingKey}. Message will be requeued.", args.RoutingKey);
            channel.BasicNack(args.DeliveryTag, false, true);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
