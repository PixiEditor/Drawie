﻿using Avalonia.Rendering.Composition;

namespace Drawie.Interop.Avalonia.Core;

public interface IDrawieInteropContext
{
    public static IDrawieInteropContext Current { get; private set; }
    public RenderApiResources CreateResources(CompositionDrawingSurface surface,
        ICompositionGpuInterop interop);

    public static void SetCurrent(IDrawieInteropContext context)
    {
        if (Current != null)
        {
            throw new System.InvalidOperationException("Context already set");
        }
        
        Current = context;
    }
}