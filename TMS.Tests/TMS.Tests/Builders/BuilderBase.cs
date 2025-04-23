namespace TMS.Tests.Builders;

internal abstract class BuilderBase<T> where T : class
{
    protected T Instance { get; set; } = null!;

    public abstract void Reset();

    public abstract T Build();
}