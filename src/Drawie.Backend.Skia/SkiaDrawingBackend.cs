﻿using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Skia.Exceptions;
using Drawie.Skia.Implementations;
using SkiaSharp;

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

            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl, imageFilterImpl, shader);
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
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl, pixmapImpl);
            BitmapImplementation = bitmapImpl;

            shader.SetBitmapImplementation(bitmapImpl);

            SkiaCanvasImplementation canvasImpl =
                new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl);

            SurfaceImplementation = new SkiaSurfaceImplementation(GraphicsContext, pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImplementation(SurfaceImplementation);
            imgImpl.SetSurfaceImplementation(SurfaceImplementation);

            CanvasImplementation = canvasImpl;
        }

        public void Setup(IRenderApi renderApi)
        {
            if (renderApi is not IVulkanRenderApi vulkanRenderApi)
            {
                throw new UnsupportedRenderApiException(renderApi);
            }

            SetGraphicsContext(vulkanRenderApi.VulkanContext);

            SurfaceImplementation.GrContext = GraphicsContext;
        }

        public DrawingSurface CreateRenderSurface(VecI size, ITexture renderTexture, SurfaceOrigin surfaceOrigin)
        {
            if(renderTexture is IVkTexture texture)
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
            
            throw new ArgumentException("Only Vulkan textures are supported at the moment");
        }

        private void SetGraphicsContext(IVulkanContext vulkanContext)
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

        private void DisposeImpl<T>(SkObjectImplementation<T> impl) where T : SKObject
        {
            impl.DisposeAll();
        }
    }
}
