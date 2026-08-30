using System.Numerics;
using System.Runtime.InteropServices;

namespace Drawie.Backend.Arco;

[StructLayout(LayoutKind.Sequential)]
public struct DrawInstance
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector4 Color { get; set; }
}