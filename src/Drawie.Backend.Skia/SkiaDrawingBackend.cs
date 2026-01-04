using System.Diagnostics;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Skia.Exceptions;
using Drawie.Skia.Implementations;
using SkiaSharp;
using ArgumentException = System.ArgumentException;

namespace Drawie.Skia
{
    public class SkiaDrawingBackend : IDrawingBackend
    {
        private IVulkanRenderApi? vulkanRenderApi;

        public GRContext? GraphicsContext
        {
            get => _grContext;
            private set
            {
                if (_grContext != null)
                {
                    throw new GrContextAlreadyInitializedException();
                }

                _grContext = value;
            }
        }

        public IPathEffectImplementation PathEffectImplementation { get; set; }
        public bool IsHardwareAccelerated => GraphicsContext != null;

        public IRenderingDispatcher RenderingDispatcher { get; set; }

        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        ISurfaceImplementation IDrawingBackend.SurfaceImplementation => SurfaceImplementation;
        public SkiaSurfaceImplementation SurfaceImplementation { get; }
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        public IColorFilterImplementation ColorFilterImplementation { get; }
        public IImageFilterImplementation ImageFilterImplementation { get; }
        public IShaderImplementation ShaderImplementation { get; set; }
        public IFontImplementation FontImplementation { get; set; }
        public IRecorderImplementation RecorderImplementation { get; }
        public IPictureImplementation PictureImplementation { get; }
        public IBlenderImplementation BlenderImplementation { get; }
        public IMeshImplementation MeshImplementation { get; }

        private GRContext _grContext;

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();

            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;

            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;

            SkiaImageFilterImplementation imageFilterImpl = new SkiaImageFilterImplementation();
            ImageFilterImplementation = imageFilterImpl;

            SkiaShaderImplementation shader = new SkiaShaderImplementation();
            ShaderImplementation = shader;

            imageFilterImpl.ShaderImplementation = shader;

            SkiaPathEffectImplementation pathEffectImpl = new SkiaPathEffectImplementation();
            PathEffectImplementation = pathEffectImpl;

            var blenderImpl = new SkiaBlenderImplementation();
            BlenderImplementation = blenderImpl;

            SkiaPaintImplementation paintImpl =
                new SkiaPaintImplementation(colorFilterImpl, imageFilterImpl, shader, pathEffectImpl, blenderImpl);
            PaintImplementation = paintImpl;

            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;

            MatrixImplementation = new SkiaMatrixImplementation();

            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;

            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl, pixmapImpl, shader);
            ImageImplementation = imgImpl;

            imageFilterImpl.ImageImplementation = imgImpl;

            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl, pixmapImpl);
            BitmapImplementation = bitmapImpl;

            shader.SetBitmapImplementation(bitmapImpl);
            shader.ImageImplementation = imgImpl;

            SkiaFontImplementation fontImpl = new SkiaFontImplementation(pathImpl);
            FontImplementation = fontImpl;

            var meshImplementation = new SkiaMeshImplementation();
            MeshImplementation = meshImplementation;

            SkiaCanvasImplementation canvasImpl =
                new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl, fontImpl, meshImplementation);

            SurfaceImplementation = new SkiaSurfaceImplementation(GraphicsContext, pixmapImpl, canvasImpl, paintImpl);


            canvasImpl.SetSurfaceImplementation(SurfaceImplementation);
            imgImpl.SetSurfaceImplementation(SurfaceImplementation);

            var pictureImplementation = new SkiaPictureImplementation(shader);
            PictureImplementation = pictureImplementation;

            RecorderImplementation = new SkiaRecorderImplementation(canvasImpl, pictureImplementation);

            CanvasImplementation = canvasImpl;
        }

        public void Setup(IRenderApi renderApi)
        {
            if (renderApi is IVulkanRenderApi vulkanRenderApi)
            {
                SetupVulkan(vulkanRenderApi.VulkanContext);
            }
            else if (renderApi is IOpenGlRenderApi openGlRenderApi)
            {
                if (openGlRenderApi.OpenGlContext.IsGlViaAngle)
                {
                    SetupAngleOpenGl(openGlRenderApi.OpenGlContext);
                }
                else
                {
                    SetupOpenGl(openGlRenderApi.OpenGlContext);
                }
            }
            else if (renderApi is IWebGlRenderApi webGlRenderApi)
            {
                SetupWebGl(webGlRenderApi.WebGlContext);
            }
            else
            {
                throw new UnsupportedRenderApiException(renderApi);
            }
        }

        private void SetupOpenGl(IOpenGlContext openGlContext)
        {
            GRGlInterface glInterface = GRGlInterface.CreateOpenGl(openGlContext.GetGlInterface);
            GraphicsContext = GRContext.CreateGl(glInterface);
            SurfaceImplementation.GrContext = GraphicsContext;
        }

        private void SetupAngleOpenGl(IOpenGlContext openGlContext)
        {
            GRGlInterface glInterface = GRGlInterface.CreateAngle(openGlContext.GetGlInterface);
            GraphicsContext = GRContext.CreateGl(glInterface, new GRContextOptions()
            {
                AvoidStencilBuffers = true
            });
            SurfaceImplementation.GrContext = GraphicsContext;
        }

        private void SetupWebGl(IWebGlContext webGlContext)
        {
            try
            {
                GRGlInterface glInterface = GRGlInterface.CreateWebGl(webGlContext.GetGlInterface);
                GraphicsContext = GRContext.CreateGl(glInterface);
                SurfaceImplementation.GrContext = GraphicsContext;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public DrawingSurface CreateRenderSurface(VecI size, ITexture renderTexture, SurfaceOrigin surfaceOrigin)
        {
            if (renderTexture is IVkTexture texture)
            {
                var imageInfo = new GRVkImageInfo()
                {
                    CurrentQueueFamily = texture.QueueFamily,
                    Format = texture.ImageFormat,
                    Image = texture.ImageHandle,
                    ImageLayout = texture.Layout,
                    ImageTiling = texture.Tiling,
                    ImageUsageFlags = texture.UsageFlags,
                    LevelCount = 1,
                    SampleCount = 1,
                    Protected = false,
                    SharingMode = texture.TargetSharingMode,
                };

                var surface = SKSurface.Create(GraphicsContext, new GRBackendRenderTarget(size.X, size.Y, 1, imageInfo),
                    (GRSurfaceOrigin)surfaceOrigin, SKColorType.Rgba8888,
                    new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

                return DrawingSurface.FromNative(surface);
            }
            else if (renderTexture is IWebGlTexture or IOpenGlTexture)
            {
                uint textureId = renderTexture switch
                {
                    IWebGlTexture wgl => wgl.TextureId,
                    IOpenGlTexture ogl => ogl.TextureId,
                    _ => throw new ArgumentException("Unsupported texture type.")
                };

                GRBackendRenderTarget backendRenderTarget = new GRBackendRenderTarget(size.X, size.Y, 1, 0,
                    new GRGlFramebufferInfo(textureId, SKColorType.Rgba8888.ToGlSizedFormat()));

                var surface = SKSurface.Create(GraphicsContext, backendRenderTarget, (GRSurfaceOrigin)surfaceOrigin,
                    SKColorType.Rgba8888);

                return DrawingSurface.FromNative(surface);
            }

            throw new ArgumentException("Unsupported texture type.");
        }

        private void SetupVulkan(IVulkanContext vulkanContext)
        {
            var vkBackendContext = new GRVkBackendContext()
            {
                VkDevice = vulkanContext.LogicalDeviceHandle,
                VkInstance = vulkanContext.InstanceHandle,
                VkPhysicalDevice = vulkanContext.PhysicalDeviceHandle,
                VkQueue = vulkanContext.GraphicsQueueHandle,
                GraphicsQueueIndex = vulkanContext.GraphicsQueueFamilyIndex,
                GetProcedureAddress = vulkanContext.GetProcedureAddress,
            };

            GraphicsContext = GRContext.CreateVulkan(vkBackendContext);
            SurfaceImplementation.GrContext = GraphicsContext;
        }

        public void Flush()
        {
            GraphicsContext?.Flush();
        }

        public override string ToString()
        {
            return "Skia";
        }

        public async ValueTask DisposeAsync()
        {
            DisposeImpl<SKCanvas>(CanvasImplementation as SkiaCanvasImplementation);
            DisposeImpl<SKPaint>(PaintImplementation as SkiaPaintImplementation);
            DisposeImpl<SKPath>(PathImplementation as SkiaPathImplementation);
            DisposeImpl<SKPixmap>(PixmapImplementation as SkiaPixmapImplementation);
            DisposeImpl<SKSurface>(SurfaceImplementation);

            if (_grContext is IAsyncDisposable grContextAsyncDisposable)
            {
                await grContextAsyncDisposable.DisposeAsync();
            }
            else
            {
                _grContext.Dispose();
            }
        }

        public int GetNativeInstancesTotalCount()
        {
            int totalCount = 0;
            totalCount += (CanvasImplementation as SkiaCanvasImplementation).Count;
            Debug.WriteLine("Canvases: " + totalCount);
            int lastCount = totalCount;
            totalCount += (PaintImplementation as SkiaPaintImplementation).Count;
            Debug.WriteLine("Paints: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (PathImplementation as SkiaPathImplementation).Count;
            Debug.WriteLine("Paths: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (PixmapImplementation as SkiaPixmapImplementation).Count;
            Debug.WriteLine("Pixmaps: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (SurfaceImplementation as SkiaSurfaceImplementation).Count;
            Debug.WriteLine("Surfaces: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ImageImplementation as SkiaImageImplementation).Count;
            Debug.WriteLine("Images: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (BitmapImplementation as SkiaBitmapImplementation).Count;
            Debug.WriteLine("Bitmaps: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ColorSpaceImplementation as SkiaColorSpaceImplementation).Count;
            Debug.WriteLine("ColorSpaces: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ColorFilterImplementation as SkiaColorFilterImplementation).Count;
            Debug.WriteLine("ColorFilters: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ImageFilterImplementation as SkiaImageFilterImplementation).Count;
            Debug.WriteLine("ImageFilters: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ShaderImplementation as SkiaShaderImplementation).Count;
            Debug.WriteLine("Shaders: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (FontImplementation as SkiaFontImplementation).Count;
            Debug.WriteLine("Fonts: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (PathEffectImplementation as SkiaPathEffectImplementation).Count;
            Debug.WriteLine("PathEffects: " + (totalCount - lastCount));
            lastCount = totalCount;
            totalCount += (ImgDataImplementation as SkiaImgDataImplementation).Count;
            Debug.WriteLine("ImgData: " + (totalCount - lastCount));
            lastCount = totalCount;
            return totalCount;
        }

        private void DisposeImpl<T>(SkObjectImplementation<T> impl) where T : SKObject
        {
            impl.DisposeAll();
        }
    }
}
