namespace Drawie.RenderApi.WebGl;

[Flags]
public enum WebGlBufferMask
{
    ColorBufferBit = 0x00004000,
    DepthBufferBit = 0x00000100,
    StencilBufferBit = 0x00000400
}
