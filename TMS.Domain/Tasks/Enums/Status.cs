using System.Text.Json.Serialization;

namespace TMS.Domain.Tasks.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    NotStarted,
    InProgress,
    Completed
}