using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces
{
    public class DrawingSurface : NativeObject, IPixelsMap
    {
        public override object Native =>
            DrawingBackendApi.Current.SurfaceImplementation.GetNativeSurface(ObjectPointer);

        public Canvas Canvas { get; private set; }

        public RectI DeviceClipBounds =>
            DrawingBackendApi.Current.SurfaceImplementation.GetDeviceClipBounds(ObjectPointer);

        public RectD LocalClipBounds =>
            DrawingBackendApi.Current.SurfaceImplementation.GetLocalClipBounds(ObjectPointer);

        public bool IsDisposed => isDisposed || Canvas.IsDisposed;

        public event SurfaceChangedEventHandler? Changed;

        private bool isDisposed;

        public DrawingSurface(IntPtr objPtr, Canvas canvas) : base(objPtr)
        {
            Canvas = canvas;
            Canvas.Changed += OnCanvasChanged;
        }

        public static DrawingSurface Create(Pixmap imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }

        public void Draw(Canvas drawingSurfaceCanvas, int x, int y, Paint drawingPaint)
        {
            DrawingBackendApi.Current.SurfaceImplementation.Draw(this, drawingSurfaceCanvas, x, y, drawingPaint);
            Changed?.Invoke(null);
        }

        public Image Snapshot()
        {
            return DrawingBackendApi.Current.ImageImplementation.Snapshot(this);
        }

        public Image Snapshot(RectI bounds)
        {
            return DrawingBackendApi.Current.ImageImplementation.Snapshot(this, bounds);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.SurfaceImplementation.PeekPixels(this);
        }

        void IPixelsMap.MarkPixelsChanged()
        {
            // noop
        }

        public bool ReadPixels(ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.ReadPixels(this, dstInfo, dstPixels, dstRowBytes,
                srcX, srcY);
        }

        public static DrawingSurface Create(ImageInfo imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixels, rowBytes);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixelBuffer);
        }

        public override void Dispose()
        {
            isDisposed = true;
            Canvas.Changed -= OnCanvasChanged;
            Canvas.Dispose(); // TODO: make sure this is correct
            DrawingBackendApi.Current.SurfaceImplementation.Dispose(this);
        }

        private void OnCanvasChanged(RectD? changedrect)
        {
            Changed?.Invoke(changedrect);
        }

        public void Flush()
        {
            DrawingBackendApi.Current.SurfaceImplementation.Flush(this);
        }

        public static DrawingSurface FromNative(object native)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.FromNative(native);
        }

        public static void Unmanage(DrawingSurface surface)
        {
            DrawingBackendApi.Current.SurfaceImplementation.Unmanage(surface);
        }

#if DEBUG
        public void SaveToDesktop()
        {
            using Surface surf = Surface.ForDisplay(DeviceClipBounds.Size);
            surf.DrawingSurface.Canvas.DrawSurface(this, 0, 0);
            surf.SaveToDesktop();
        }
#endif
    }
}
