using TMS.Application.Tasks.DTOs;
using TMS.Domain.Tasks.Entities;

namespace TMS.Application.Tasks.Mappers;

public static class TaskDtoMapper
{
    public static TaskDto ToDto(TaskItem entity) => new(entity.Id ?? 0, entity.Name, entity.Description, entity.Status.ToString());
}