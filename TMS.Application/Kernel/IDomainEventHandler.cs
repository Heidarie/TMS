using TMS.Domain.Kernel;

namespace TMS.Application.Kernel;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : class, IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent);
}