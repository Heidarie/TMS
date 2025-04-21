using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TMS.Application.Messaging;
using TMS.Application.Tasks.Messages;
using TMS.Infrastructure.Messaging.RabbitMq;

namespace TMS.Infrastructure.Messaging;

public class CompletedTaskMessageConsumerBackgroundService(ConnectionFactory factory, RabbitMqOptions options, ILogger<CompletedTaskMessageConsumerBackgroundService> logger) : BackgroundService
{
    private const string TaskCompletedQueueName = "task.completed";

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var connection = await factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(options.ExchangeName, ExchangeType.Topic, true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(options.QueueName, true, false, false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(options.QueueName, options.ExchangeName, TaskCompletedQueueName, null, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        logger.LogInformation("Listening for messages on queue: {QueueName}", options.QueueName);

        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = System.Text.Json.JsonSerializer.Deserialize<TaskCompletedMessage>(body)!;
            
            logger.LogInformation("Received task completion event. Id: {Id}, Name: {Name}, Description: {Description}", message.Id, message.Name, message.Description);
            
            channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: cancellationToken);
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(options.QueueName, false, consumer, cancellationToken: cancellationToken);
    }
}