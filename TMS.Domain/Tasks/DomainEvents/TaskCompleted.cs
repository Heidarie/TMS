using TMS.Domain.Kernel;
using TMS.Domain.Tasks.Entities;

namespace TMS.Domain.Tasks.DomainEvents;

public record TaskCompleted(TaskItem Task) : IDomainEvent;