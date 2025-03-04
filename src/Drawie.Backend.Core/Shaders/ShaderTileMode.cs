﻿namespace Drawie.Backend.Core.Shaders;

public enum ShaderTileMode
{
    /// <summary>Replicate the edge color.</summary>
    Clamp,

    /// <summary>Repeat the shader's image horizontally and vertically.</summary>
    Repeat,

    /// <summary>Repeat the shader's image horizontally and vertically, alternating mirror images so that adjacent images always seam.</summary>
    Mirror,

    /// <summary>To be added.</summary>
    Decal,
}
