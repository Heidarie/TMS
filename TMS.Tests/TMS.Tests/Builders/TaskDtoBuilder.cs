using TMS.Application.Tasks.DTOs;
using TMS.Domain.Tasks.Enums;

namespace TMS.Tests.Builders;

internal class TaskDtoBuilder : BuilderBase<TaskDto>
{
    public TaskDtoBuilder()
    {
        Instance = new TaskDto(0, string.Empty, string.Empty, Status.NotStarted.ToString());
    }

    public TaskDtoBuilder WithId(int id)
    {
        Instance = Instance with {Id = id};
        return this;
    }

    public TaskDtoBuilder WithName(string name)
    {
        Instance = Instance with { Name = name };
        return this;
    }

    public TaskDtoBuilder WithDescription(string description)
    {
        Instance = Instance with { Description = description };
        return this;
    }

    public TaskDtoBuilder WithStatus(Status status)
    {
        Instance = Instance with { Status = status.ToString() };
        return this;
    }

    public override void Reset()
    {
        Instance = new TaskDto(0, string.Empty, string.Empty, Status.NotStarted.ToString());
    }

    public override TaskDto Build()
    {
        var result = Instance;
        Reset();
        return result;
    }
}