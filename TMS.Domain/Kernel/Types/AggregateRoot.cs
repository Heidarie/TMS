namespace TMS.Domain.Kernel.Types;

public abstract class AggregateRoot<T>
{
    public T Id { get; protected set; } = default!;
    public int Version { get; protected set; }
    public IEnumerable<IDomainEvent> DomainEvents => _events;

    private readonly List<IDomainEvent> _events = new();
    private bool _versionIncremented;

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_events.Any() && !_versionIncremented)
        {
            Version++;
            _versionIncremented = true;
        }

        _events.Add(domainEvent);
    }

    public void ClearDomainEvents() => _events.Clear();
    
    protected void IncrementVersion()
    {
        if (_versionIncremented)
        {
            return;
        }

        Version++;
        _versionIncremented = true;
    }
}

public class AggregateRoot : AggregateRoot<AggregateId>
{
}