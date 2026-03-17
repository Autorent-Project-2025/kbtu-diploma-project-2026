using RabbitMQ.Client;

namespace AutoRent.Messaging.RabbitMq;

public static class RabbitMqConnectionFactoryBuilder
{
    public static ConnectionFactory Build(RabbitMqOptions options, bool dispatchConsumersAsync = false)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.HostName))
        {
            throw new InvalidOperationException("RabbitMq:HostName is required.");
        }

        if (options.Port <= 0)
        {
            throw new InvalidOperationException("RabbitMq:Port must be greater than 0.");
        }

        return new ConnectionFactory
        {
            HostName = options.HostName.Trim(),
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = string.IsNullOrWhiteSpace(options.VirtualHost) ? "/" : options.VirtualHost.Trim(),
            AutomaticRecoveryEnabled = true,
            DispatchConsumersAsync = dispatchConsumersAsync
        };
    }
}
