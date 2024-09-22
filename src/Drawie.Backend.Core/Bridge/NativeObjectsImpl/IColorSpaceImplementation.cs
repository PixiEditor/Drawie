﻿using Drawie.Backend.Core.Surfaces.ImageData;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IColorSpaceImplementation
{
    public ColorSpace CreateSrgb();
    public void Dispose(IntPtr objectPointer);
    public object GetNativeColorSpace(IntPtr objectPointer);
}