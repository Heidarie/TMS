using TMS.Application.Kernel;
using TMS.Application.Messaging;
using TMS.Application.Tasks.Messages;
using TMS.Domain.Tasks.DomainEvents;
using TMS.Domain.Tasks.Enums;

namespace TMS.Infrastructure.Tasks.DomainEvents.Handlers;

class TaskCompletedHandler(IMessageBroker messageBroker) : IDomainEventHandler<TaskCompleted>
{
    private const string RoutingKey = "task.completed";

    public async Task HandleAsync(TaskCompleted domainEvent)
    {
        if (domainEvent.Task.Status is not Status.Completed)
        {
            return;
        }

        var message = new TaskCompletedMessage(domainEvent.Task.Id, domainEvent.Task.Name, domainEvent.Task.Description);

        await messageBroker.PublishAsync(message, RoutingKey);
    }
}