using RabbitMQ.Client;

namespace AutoRent.Messaging.RabbitMq;

public static class RabbitMqTopology
{
    public const string ExchangeName = "autorent.events";
    public const string EmailNotificationsQueue = "email-service.notifications";
    public const string BookingPaymentsQueue = "payment-service.booking-payments";
    public const string PartnerCarProvisionQueue = "car-service.partner-car-provision";

    public static class RoutingKeys
    {
        public const string TicketClientApprovedEmailRequested = "ticket.email.client-approved";
        public const string TicketClientRejectedEmailRequested = "ticket.email.client-rejected";
        public const string TicketPartnerApprovedEmailRequested = "ticket.email.partner-approved";
        public const string TicketPartnerRejectedEmailRequested = "ticket.email.partner-rejected";
        public const string TicketPartnerCarApprovedEmailRequested = "ticket.email.partner-car-approved";
        public const string TicketPartnerCarRejectedEmailRequested = "ticket.email.partner-car-rejected";
        public const string TicketPartnerCarProvisionRequested = "ticket.partner-car.provision-requested";
        public const string BookingPaymentConfirmed = "booking.payment.confirmed";
        public const string BookingPaymentCanceled = "booking.payment.canceled";
        public const string BookingPaymentCompleted = "booking.payment.completed";
    }

    public static string ResolveExchangeName(RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrWhiteSpace(options.ExchangeName)
            ? ExchangeName
            : options.ExchangeName.Trim();
    }

    public static void DeclareExchange(IModel channel, RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(channel);

        channel.ExchangeDeclare(
            exchange: ResolveExchangeName(options),
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public static void DeclareBoundQueue(
        IModel channel,
        RabbitMqOptions options,
        string queueName,
        params string[] routingKeys)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name is required.", nameof(queueName));
        }

        DeclareExchange(channel, options);

        channel.QueueDeclare(
            queue: queueName.Trim(),
            durable: true,
            exclusive: false,
            autoDelete: false);

        foreach (var routingKey in routingKeys.Where(key => !string.IsNullOrWhiteSpace(key)).Distinct())
        {
            channel.QueueBind(
                queue: queueName.Trim(),
                exchange: ResolveExchangeName(options),
                routingKey: routingKey.Trim());
        }
    }
}
