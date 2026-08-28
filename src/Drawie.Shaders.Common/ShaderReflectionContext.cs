using System.Text.Json;
using System.Text.Json.Serialization;

namespace Drawie.Backend.Shaders.Common;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ShaderReflection))]
public partial class ShaderReflectionContext : JsonSerializerContext
{
}