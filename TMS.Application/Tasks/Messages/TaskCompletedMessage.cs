using TMS.Application.Messaging;

namespace TMS.Application.Tasks.Messages;

public record TaskCompletedMessage(int Id, string Name, string Description) : IMessage;