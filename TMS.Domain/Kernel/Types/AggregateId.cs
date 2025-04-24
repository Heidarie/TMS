namespace TMS.Domain.Kernel.Types;

public class AggregateId<T> : IEquatable<AggregateId<T>>
{
    public T Value { get; private set; }
    public AggregateId(T value)
    {
        Value = value;
    }

    public bool Equals(AggregateId<T>? other)
    {
        if(ReferenceEquals(null, other)) return false;
        if(ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }
    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AggregateId<T>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value!);
    }
}

public class AggregateId : AggregateId<int>
{
    public AggregateId() : base(default) { }
    public AggregateId(int value) : base(value) { }

    public static implicit operator int(AggregateId id) => id.Value;
    public static implicit operator AggregateId(int id) => new(id);
}