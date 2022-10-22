﻿using Luo_Painter.Brushes;
using Luo_Painter.Elements;
using Luo_Painter.Layers;
using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter.Controls
{
    public sealed partial class PaintScrollViewer : UserControl, IInkParameter
    {

        private void ConstructCanvas()
        {
            this.InkCanvasControl.CreateResources += (sender, args) =>
            {
                args.TrackAsyncAction(this.CreateResourcesAsync(sender).AsAsyncAction());
            };
            this.InkCanvasControl.Draw += (sender, args) =>
            {
                switch (this.InkType)
                {
                    case InkType.Blur:
                        args.DrawingSession.DrawImage(InkPresenter.GetBlur(this.InkRender, this.InkPresenter.Flow * 4));
                        break;
                    case InkType.Mosaic:
                        args.DrawingSession.DrawImage(InkPresenter.GetMosaic(this.InkRender, this.InkPresenter.Size / 10));
                        break;
                    default:
                        args.DrawingSession.DrawImage(this.InkRender);
                        break;
                }
            };
        }

    }

    public sealed partial class PaletteMenu : Expander, IInkParameter
    {

        private void ConstructCanvas()
        {
            this.CanvasControl.SizeChanged += (s, e) =>
            {
                if (e.NewSize == Size.Empty) return;
                if (e.NewSize == e.PreviousSize) return;

                if (this.ShaderCodeByteIsEnabled is false) return;
                Vector2 size = this.CanvasControl.Dpi.ConvertDipsToPixels(e.NewSize.ToVector2());
                this.CreateResources((int)size.X, (int)size.Y);
            };


            this.CanvasControl.CreateResources += (sender, args) =>
            {
                float sizeX = sender.Dpi.ConvertDipsToPixels((float)sender.ActualWidth);
                float sizeY = sender.Dpi.ConvertDipsToPixels((float)sender.ActualHeight);
                this.CreateResources((int)sizeX, (int)sizeY);
                args.TrackAsyncAction(this.CreateResourcesAsync().AsAsyncAction());
            };
            this.CanvasControl.Draw += (sender, args) =>
            {
                //@DPI 
                args.DrawingSession.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">

                args.DrawingSession.DrawImage(this.BitmapLayer[BitmapType.Source]);
            };
        }

    }
}