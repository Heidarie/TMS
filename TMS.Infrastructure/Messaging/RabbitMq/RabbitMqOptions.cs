namespace TMS.Infrastructure.Messaging.RabbitMq;

public class RabbitMqOptions
{
    public string ExchangeName { get; private set; } = null!;
    public string QueueName { get; private set; } = null!;
}