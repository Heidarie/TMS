using Microsoft.Extensions.DependencyInjection;
using TMS.Application.Kernel;
using TMS.Domain.Kernel;

namespace TMS.Infrastructure.Kernel;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(params IDomainEvent[]? domainEvents)
    {
        if (domainEvents is null || !domainEvents.Any())
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();

        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = scope.ServiceProvider.GetServices(handlerType);

            var tasks = handlers.Select(x => (Task)handlerType
                .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))?.Invoke(x, new object?[] { domainEvent })!);

            await Task.WhenAll(tasks);
        }
    }
}