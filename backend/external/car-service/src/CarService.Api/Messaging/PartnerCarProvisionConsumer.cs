using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using CarService.Application.DTOs.PartnerCars;
using CarService.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarService.Api.Messaging;

public sealed class PartnerCarProvisionConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PartnerCarProvisionConsumer> _logger;
    private readonly RabbitMqOptions _options;

    public PartnerCarProvisionConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<PartnerCarProvisionConsumer> logger)
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
                    RabbitMqTopology.PartnerCarProvisionQueue,
                    RabbitMqTopology.RoutingKeys.TicketPartnerCarProvisionRequested);

                channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, args) => await HandleMessageAsync(channel, args, stoppingToken);

                var consumerTag = channel.BasicConsume(
                    queue: RabbitMqTopology.PartnerCarProvisionQueue,
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
                _logger.LogError(ex, "Partner car RabbitMQ consumer failed. Restarting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var message = RabbitMqJson.Deserialize<PartnerCarProvisionRequested>(args.Body)
                ?? throw new InvalidOperationException("Partner car provision message is invalid.");

            using var scope = _scopeFactory.CreateScope();
            var partnerCarService = scope.ServiceProvider.GetRequiredService<IPartnerCarService>();

            await partnerCarService.ProvisionAsync(
                new PartnerCarProvisionDto
                {
                    RelatedUserId = message.Payload.RelatedUserId,
                    ProvisionRequestKey = message.Payload.ProvisionRequestKey,
                    CarBrand = message.Payload.CarBrand,
                    CarModel = message.Payload.CarModel,
                    CarYear = message.Payload.CarYear,
                    LicensePlate = message.Payload.LicensePlate,
                    PriceHour = message.Payload.PriceHour,
                    PriceDay = message.Payload.PriceDay,
                    OwnershipFileName = message.Payload.OwnershipDocumentFileName,
                    Images = message.Payload.Images
                        .Select(image => new PartnerCarProvisionImageDto
                        {
                            ImageId = image.ImageId,
                            ImageUrl = image.ImageUrl
                        })
                        .ToArray()
                },
                cancellationToken);

            channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Partner car consumer failed for routing key {RoutingKey}. Message will be requeued.", args.RoutingKey);
            channel.BasicNack(args.DeliveryTag, false, true);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
