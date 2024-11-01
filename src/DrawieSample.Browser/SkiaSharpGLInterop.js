var SkiaSharpGLInterop = {
    $SkiaSharpLibrary: {
        internal_func: function () {
        }
    },
    InterceptGLObject: function () {
        globalThis.SkiaSharpGL = GL
    }
}

autoAddDeps(SkiaSharpGLInterop, '$SkiaSharpLibrary')
mergeInto(LibraryManager.library, SkiaSharpGLInterop)
