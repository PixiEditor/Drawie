using System.Numerics;
using System.Runtime.InteropServices;

namespace Drawie.Backend.Arco;

[StructLayout(LayoutKind.Sequential)]
public struct Globals
{
    public Vector2 ViewportSize { get; set; }
}