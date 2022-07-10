﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using System;
using System.Numerics;
using Windows.UI;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Storage.Streams;

namespace Luo_Painter.Layers.Models
{
    public sealed partial class BitmapLayer : LayerBase, ILayer
    {
        public LayerType Type => LayerType.Bitmap;


        public ICanvasImage this[BitmapType type]
        {
            get
            {
                switch (type)
                {
                    case BitmapType.Origin: return this.OriginRenderTarget;
                    case BitmapType.Temp: return this.TempRenderTarget;
                    default: return this.SourceRenderTarget;
                }
            }
        }

        readonly CanvasRenderTarget OriginRenderTarget;
        readonly CanvasRenderTarget SourceRenderTarget;
        readonly CanvasRenderTarget TempRenderTarget;

        readonly byte[] Pixels;
        readonly IBuffer Buffer;


        //@Construct
        public BitmapLayer(ICanvasResourceCreator resourceCreator, BitmapLayer bitmapLayer) : this(resourceCreator, bitmapLayer.SourceRenderTarget, bitmapLayer.Width, bitmapLayer.Height) { }
        public BitmapLayer(ICanvasResourceCreator resourceCreator, BitmapLayer bitmapLayer, int width, int height) : this(resourceCreator, bitmapLayer.SourceRenderTarget, width, height) { }
        public BitmapLayer(ICanvasResourceCreator resourceCreator, CanvasBitmap bitmap) : this(resourceCreator, bitmap, (int)bitmap.SizeInPixels.Width, (int)bitmap.SizeInPixels.Height) { }

        public BitmapLayer(ICanvasResourceCreator resourceCreator, ICanvasImage image, int width, int height) : this(resourceCreator, width, height)
        {
            using (CanvasDrawingSession ds = this.OriginRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(image);
            }
            using (CanvasDrawingSession ds = this.SourceRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(image);
            }

            this.RenderThumbnail();
        }

        public BitmapLayer(ICanvasResourceCreator resourceCreator, BitmapLayer bitmapLayer, Vector2 offset) : this(resourceCreator, bitmapLayer.SourceRenderTarget, bitmapLayer.Width, bitmapLayer.Height, offset) { }
        public BitmapLayer(ICanvasResourceCreator resourceCreator, BitmapLayer bitmapLayer, int width, int height, Vector2 offset) : this(resourceCreator, bitmapLayer.SourceRenderTarget, width, height, offset) { }
        public BitmapLayer(ICanvasResourceCreator resourceCreator, CanvasBitmap bitmap, Vector2 offset) : this(resourceCreator, bitmap, (int)bitmap.SizeInPixels.Width, (int)bitmap.SizeInPixels.Height, offset) { }

        public BitmapLayer(ICanvasResourceCreator resourceCreator, ICanvasImage image, int width, int height, Vector2 offset) : this(resourceCreator, width, height)
        {
            using (CanvasDrawingSession ds = this.OriginRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(image, offset);
            }
            using (CanvasDrawingSession ds = this.SourceRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(image, offset);
            }

            this.RenderThumbnail();
        }

        public BitmapLayer(ICanvasResourceCreator resourceCreator, int width, int height) : base(resourceCreator, width, height)
        {
            //@DPI
            this.OriginRenderTarget = new CanvasRenderTarget(resourceCreator, width, height, 96);
            this.SourceRenderTarget = new CanvasRenderTarget(resourceCreator, width, height, 96);
            this.TempRenderTarget = new CanvasRenderTarget(resourceCreator, width, height, 96);

            this.Pixels = new byte[width * height * 4];
            this.Buffer = this.Pixels.AsBuffer();


            this.XDivisor = width / BitmapLayer.Unit;
            this.YDivisor = height / BitmapLayer.Unit;
            this.XRemainder = width % BitmapLayer.Unit;
            this.YRemainder = height % BitmapLayer.Unit;


            this.XLength = this.XDivisor;
            this.YLength = this.YDivisor;
            if (this.XRemainder is 0 is false)
            {
                this.RegionType |= RegionType.XRemainder;
                this.XLength += 1;
            }
            if (this.YRemainder is 0 is false)
            {
                this.RegionType |= RegionType.YRemainder;
                this.YLength += 1;
            }


            this.Hits = new bool[this.XLength * this.YLength];

            this.Bounds = new PixelBounds
            {
                Left = 0,
                Top = 0,
                Right = width,
                Bottom = height,
            };
            this.Interpolation = new CanvasRenderTarget(resourceCreator, this.XLength, this.YLength, 96);
        }

        public ICanvasImage Render(ICanvasImage background) => base.Render(background, this.SourceRenderTarget);
        public ICanvasImage Render(ICanvasImage background, Matrix3x2 matrix, CanvasImageInterpolation interpolationMode) => base.Render(background, new Transform2DEffect
        {
            InterpolationMode = interpolationMode,
            TransformMatrix = matrix,
            Source = this.SourceRenderTarget,
        });
        public ICanvasImage Render(ICanvasImage background, Matrix3x2 matrix, CanvasImageInterpolation interpolationMode, string id, ICanvasImage mezzanine) => base.Render(background, new Transform2DEffect
        {
            InterpolationMode = interpolationMode,
            TransformMatrix = matrix,
            Source = (base.Id == id) ? mezzanine : this.SourceRenderTarget,
        });

        public ICanvasImage Merge(ILayerRender previousRender, ICanvasImage previousImage)
        {
            if (base.Opacity == 0.0) return null;
            else if (base.Opacity == 1.0) return previousRender.Render(previousImage, this.SourceRenderTarget);
            return previousRender.Render(previousImage, new OpacityEffect
            {
                Opacity = base.Opacity,
                Source = this.SourceRenderTarget
            });
        }

        public void RenderThumbnail() => base.RenderThumbnail(this.SourceRenderTarget);

        public void Flush() => this.OriginRenderTarget.CopyPixelsFromBitmap(this.SourceRenderTarget);

        public void CopyPixels(BitmapLayer source) => this.SourceRenderTarget.CopyPixelsFromBitmap(source.SourceRenderTarget);

        public CanvasDrawingSession CreateDrawingSession(BitmapType type = BitmapType.Source)
        {
            switch (type)
            {
                case BitmapType.Origin: return this.OriginRenderTarget.CreateDrawingSession();
                case BitmapType.Source: return this.SourceRenderTarget.CreateDrawingSession();
                case BitmapType.Temp: return this.TempRenderTarget.CreateDrawingSession();
                default: return this.SourceRenderTarget.CreateDrawingSession();
            }
        }

        public void Draw(ICanvasImage image, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.DrawImage(image);
            }
        }
        public void Draw(ICanvasImage image, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            using (ds.CreateLayer(1, clipRectangle))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.DrawImage(image);
            }
        }

        public void DrawCopy(ICanvasImage image, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.DrawImage(image);
            }
        }
        public void DrawCopy(ICanvasImage image, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            using (ds.CreateLayer(1, clipRectangle))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.DrawImage(image);
            }
        }
        public void DrawCopy(ICanvasImage image1, ICanvasImage image2, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.DrawImage(image1);
                ds.Blend = CanvasBlend.SourceOver;
                ds.DrawImage(image2);
            }
        }
        public void DrawCopy(ICanvasImage image1, ICanvasImage image2, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            using (ds.CreateLayer(1, clipRectangle))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.DrawImage(image1);
                ds.Blend = CanvasBlend.SourceOver;
                ds.DrawImage(image2);
            }
        }

        public void Fill(ICanvasBrush brush, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.FillRectangle(0, 0, this.Width, this.Height, brush);
            }
        }
        public void Fill(ICanvasBrush brush, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.FillRectangle(clipRectangle, brush);
            }
        }

        public void Clear(ICanvasBrush brush, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.FillRectangle(0, 0, this.Width, this.Height, brush);
            }
        }
        public void Clear(ICanvasBrush brush, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                //@DPI 
                ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                ds.Blend = CanvasBlend.Copy;
                ds.FillRectangle(clipRectangle, brush);
            }
        }

        public void Clear(Color color, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            {
                ds.Clear(color);
            }
        }
        public void Clear(Color color, Rect clipRectangle, BitmapType type = BitmapType.Source)
        {
            using (CanvasDrawingSession ds = this.CreateDrawingSession(type))
            using (ds.CreateLayer(1, clipRectangle))
            {
                ds.Clear(color);
            }
        }

    }
}