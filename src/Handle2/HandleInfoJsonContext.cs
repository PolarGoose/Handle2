using Handle2.HandleInfo;
using System.Text.Json.Serialization;

namespace Handle2;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(IEnumerable<ProcessInfo>))]
internal partial class HandleInfoJsonContext : JsonSerializerContext
{
}
