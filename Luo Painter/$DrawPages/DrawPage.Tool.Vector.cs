﻿using Luo_Painter.Elements;
using System.Numerics;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page
    {

        private void ConstructVector()
        {
            this.ViewTool.RadianValueChanged += (s, radian) =>
            {
                this.Transformer.Radian = (float)radian;
                this.Transformer.ReloadMatrix();
                this.CanvasControl.Invalidate(); // Invalidate
            };

            this.ViewTool.ScaleValueChanged += (s, scale) =>
            {
                this.Transformer.Scale = (float)scale;
                this.Transformer.ReloadMatrix();
                this.CanvasControl.Invalidate(); // Invalidate
            };
            
            this.ViewTool.RemoteControl.Moved += (s, vector) =>
            {
                this.Transformer.Position += vector * 20;
                this.Transformer.ReloadMatrix();
                this.CanvasControl.Invalidate(); // Invalidate
            };
            this.ViewTool.RemoteControl.ValueChangeStarted += (s, value) => this.Transformer.CacheMove(Vector2.Zero);
            this.ViewTool.RemoteControl.ValueChangeDelta += (s, value) =>
            {
                this.Transformer.Move(value);
                this.CanvasControl.Invalidate(); // Invalidate
            };
            this.ViewTool.RemoteControl.ValueChangeCompleted += (s, value) =>
            {
                this.Transformer.Move(value);
                this.CanvasControl.Invalidate(); // Invalidate
            };
        }

        private void View_Start(Vector2 point)
        {
            this.Transformer.CacheMove(this.CanvasControl.Dpi.ConvertDipsToPixels(point));
            this.CanvasControl.Invalidate(); // Invalidate
        }
        private void View_Delta(Vector2 point)
        {
            this.Transformer.Move(this.CanvasControl.Dpi.ConvertDipsToPixels(point));
            this.CanvasControl.Invalidate(); // Invalidate
        }
        private void View_Complete(Vector2 point)
        {
            this.Transformer.Move(this.CanvasControl.Dpi.ConvertDipsToPixels(point));
            this.CanvasControl.Invalidate(); // Invalidate

            this.ViewTool.Construct(this.Transformer);
        }

    }
}