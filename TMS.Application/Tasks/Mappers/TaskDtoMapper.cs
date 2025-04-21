using TMS.Application.Tasks.DTOs;
using TMS.Domain.Tasks.Entities;
using TMS.Domain.Tasks.Enums;

namespace TMS.Application.Tasks.Mappers;

public static class TaskDtoMapper
{
    public static TaskDto ToDto(TaskItem entity) => new(entity.Id, entity.Name, entity.Description, entity.Status.ToString());

    public static TaskItem ToEntity(TaskDto dto) => new(dto.Id, dto.Name, dto.Description, Enum.Parse<Status>(dto.Status));
}