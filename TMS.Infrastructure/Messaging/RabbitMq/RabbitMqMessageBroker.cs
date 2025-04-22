using RabbitMQ.Client;
using TMS.Application.Messaging;

namespace TMS.Infrastructure.Messaging.RabbitMq;

internal sealed class RabbitMqMessageBroker(ConnectionFactory factory, RabbitMqOptions options) : IMessageBroker
{
    public async Task PublishAsync<T>(T message, string routingKey) where T : class, IMessage
    {
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(options.ExchangeName, ExchangeType.Topic, true);

        var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(options.ExchangeName, routingKey, body);
    }
}