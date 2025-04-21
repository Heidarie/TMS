namespace TMS.Application.Messaging;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string routingKey) where T : class, IMessage;
}