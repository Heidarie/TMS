using Microsoft.Extensions.Logging;
using TMS.Application.Kernel;
using TMS.Application.Messaging;
using TMS.Application.Tasks.Messages;
using TMS.Domain.Tasks.DomainEvents;
using TMS.Domain.Tasks.Enums;

namespace TMS.Infrastructure.Tasks.DomainEvents.Handlers;

class TaskCompletedHandler(IMessageBroker messageBroker, ILogger<TaskCompletedHandler> logger) : IDomainEventHandler<TaskCompleted>
{
    public async Task HandleAsync(TaskCompleted domainEvent)
    {
        if (domainEvent.Task.Status is not Status.Completed)
        {
            return;
        }

        logger.LogInformation("Creating message for: {TaskId}", domainEvent.Task.Id.Value);

        var message = new TaskCompletedMessage(domainEvent.Task.Id, domainEvent.Task.Name, domainEvent.Task.Description);

        await messageBroker.PublishAsync(message, message.RoutingKey);
    }
}