using TMS.Domain.Kernel.Types;
using TMS.Domain.Tasks.DomainEvents;
using TMS.Domain.Tasks.Enums;

namespace TMS.Domain.Tasks.Entities;

public class TaskItem : AggregateRoot
{
    public TaskItem() { }

    internal TaskItem(AggregateId id) => Id = id;

    public static TaskItem Create(string name, string description)
    {
        var taskItem = new TaskItem(0);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.", nameof(description));

        taskItem.Name = name;
        taskItem.Description = description;
        return taskItem;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
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

        IncrementVersion();
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