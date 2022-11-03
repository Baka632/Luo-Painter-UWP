﻿using FanKit.Transformers;
using Luo_Painter.Brushes;
using Luo_Painter.Elements;
using Luo_Painter.Layers;
using Luo_Painter.Options;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page, ILayerManager, IInkParameter
    {

        bool IsViewEnabled;

        private void ConstructVector()
        {
            this.RadianClearButton.Tapped += (s, e) => this.RadianStoryboard.Begin(); // Storyboard
            this.RadianSlider.ValueChanged += (s, e) =>
            {
                if (this.IsViewEnabled is false) return;
                double radian = e.NewValue / 180 * System.Math.PI;

                this.Transformer.Radian = (float)radian;
                this.Transformer.ReloadMatrix();

                this.CanvasControl.Invalidate(); // Invalidate
                this.CanvasVirtualControl.Invalidate(); // Invalidate
            };

            this.ScaleClearButton.Tapped += (s, e) => this.ScaleStoryboard.Begin(); // Storyboard
            this.ScaleSlider.Minimum = this.ScaleRange.XRange.Minimum;
            this.ScaleSlider.Maximum = this.ScaleRange.XRange.Maximum;
            this.ScaleSlider.ValueChanged += (s, e) =>
            {
                if (this.IsViewEnabled is false) return;
                double scale = this.ScaleRange.ConvertXToY(e.NewValue);

                this.Transformer.Scale = (float)scale;
                this.Transformer.ReloadMatrix();

                this.CanvasControl.Invalidate(); // Invalidate
                this.CanvasVirtualControl.Invalidate(); // Invalidate
            };

            this.RemoteControl.Moved += (s, vector) =>
            {
                this.Transformer.Position += vector * 20;
                this.Transformer.ReloadMatrix();

                this.CanvasControl.Invalidate(); // Invalidate
                this.CanvasVirtualControl.Invalidate(); // Invalidate
            };
            this.RemoteControl.ValueChangeStarted += (s, value) => this.Transformer.CacheMove(Vector2.Zero);
            this.RemoteControl.ValueChangeDelta += (s, value) =>
            {
                this.Transformer.Move(value);

                this.CanvasControl.Invalidate(); // Invalidate
                this.CanvasVirtualControl.Invalidate(); // Invalidate
            };
            this.RemoteControl.ValueChangeCompleted += (s, value) =>
            {
                this.Transformer.Move(value);

                this.CanvasControl.Invalidate(); // Invalidate
                this.CanvasVirtualControl.Invalidate(); // Invalidate
            };
        }

        public void ConstructView(CanvasTransformer transformer)
        {
            this.IsViewEnabled = false;

            this.RadianSlider.Value = transformer.Radian * 180 / System.Math.PI;
            this.ScaleSlider.Value = this.ScaleRange.ConvertYToX(transformer.Scale);

            this.IsViewEnabled = true;
        }

        private void View_Start()
        {
            this.Transformer.CacheMove(this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(this.StartingPoint));

            this.SetCanvasState(true);
        }
        private void View_Delta()
        {
            this.Transformer.Move(this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(this.Point));

            this.CanvasControl.Invalidate(); // Invalidate
            this.CanvasVirtualControl.Invalidate(); // Invalidate
        }
        private void View_Complete()
        {
            this.Transformer.Move(this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(this.Point));

            this.SetCanvasState(this.OptionType.HasPreview());

            this.ConstructView(this.Transformer);
        }

    }
}