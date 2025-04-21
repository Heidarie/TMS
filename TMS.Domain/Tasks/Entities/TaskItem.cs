using TMS.Domain.Kernel.Types;
using TMS.Domain.Tasks.DomainEvents;
using TMS.Domain.Tasks.Enums;

namespace TMS.Domain.Tasks.Entities;

public class TaskItem : AggregateRoot
{
    public TaskItem() { }
    public TaskItem(int id, string name, string description, Status status, int version = 0) : this(id)
    {
        Name = name;
        Description = description;
        Status = status;
        Version = version;
    }

    internal TaskItem(AggregateId id) => Id = id;

    public static TaskItem Create(string name, string description)
    {
        var taskItem = new TaskItem(default!);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.", nameof(description));

        taskItem.Name = name;
        taskItem.Description = description;
        return taskItem;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public Status Status { get; private set; } = Status.NotStarted;

    public void UpdateProgress()
    {
        switch (Status)
        {
            case Status.NotStarted:
                MarkInProgress();
                break;
            case Status.InProgress:
                MarkCompleted();
                break;
            default:
                throw new InvalidOperationException("Task is already completed.");
        };
    }

    private void MarkInProgress()
    {
        if (Status != Status.NotStarted)
            throw new InvalidOperationException("Task is already in progress or completed.");
        Status = Status.InProgress;
    }

    private void MarkCompleted()
    {
        if (Status != Status.InProgress)
            throw new InvalidOperationException("Task is not in progress.");
        Status = Status.Completed;
        AddDomainEvent(new TaskCompleted(this));
    }
}