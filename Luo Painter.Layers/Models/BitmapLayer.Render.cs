﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Luo_Painter.Layers.Models
{
    public sealed partial class BitmapLayer : LayerBase, ILayer
    {
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