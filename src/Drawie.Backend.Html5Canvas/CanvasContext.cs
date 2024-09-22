﻿using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.JSInterop;

namespace Drawie.Html5Canvas;

public class CanvasContext
{
    public int ContextHandle { get; }

    public CanvasContext(int contextHandle)
    {
        ContextHandle = contextHandle;
    }

    public void ClearRect(double x, double y, double width, double height)
    {
        JSRuntime.ClearCanvasRect(ContextHandle, x, y, width, height);
    }

    public void DrawStrokeRect(int x, int y, int width, int height)
    {
        JSRuntime.StrokeCanvasRect(ContextHandle, x, y, width, height);
    }

    public void DrawFillRect(int x, int y, int width, int height)
    {
        JSRuntime.FillCanvasRect(ContextHandle, x, y, width, height);
    }

    public void SetFillStyle(string color)
    {
        JSRuntime.SetFillStyle(ContextHandle, color);
    }

    public void SetStrokeStyle(string color)
    {
        JSRuntime.SetStrokeStyle(ContextHandle, color);
    }
}