using TMS.Domain.Kernel;

namespace TMS.Application.Kernel;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(params IDomainEvent[]? domainEvents);
}