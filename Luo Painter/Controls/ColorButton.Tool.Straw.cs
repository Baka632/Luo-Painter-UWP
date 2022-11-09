﻿using Luo_Painter.Brushes;
using Luo_Painter.Layers;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter.Controls
{
    public sealed partial class ColorButton : Button, IInkParameter
    {

        private void Straw_Start()
        {
            int x = (int)this.StartingPosition.X;
            int y = (int)this.StartingPosition.Y;
            this.SolidColorBrush.Color = this.Straw(x, y);
        }
        private void Straw_Delta()
        {
            int x = (int)this.Position.X;
            int y = (int)this.Position.Y;
            this.SolidColorBrush.Color = this.Straw(x, y);
        }
        private void Straw_Complete()
        {
            int x = (int)this.Position.X;
            int y = (int)this.Position.Y;

            this.ColorPicker.ColorChanged -= this.ColorPicker_ColorChanged;
            {
                this.SolidColorBrush.Color = this.ColorPicker.Color = this.Straw(x, y);
            }
            this.ColorPicker.ColorChanged += this.ColorPicker_ColorChanged;
        }

        private Color Straw(int x, int y)
        {
            Color color = this.BitmapLayer.GetPixelColor(x, y, BitmapType.Source);
            switch (color.A)
            {
                case byte.MinValue:
                    return Colors.White;
                case byte.MaxValue:
                    return color;
                default:
                    {
                        float rAlpha = (255f - color.A) / 255f; // 1~0
                        Vector3 rgbHdr = new Vector3(color.R, color.G, color.B) / 255f; // 0~1

                        Vector3 blendWhiteHdr = rgbHdr * rAlpha; // Blend with White
                        Color blendWhite = Color.FromArgb(byte.MaxValue, (byte)(blendWhiteHdr.X / 255f), (byte)(blendWhiteHdr.Y / 255f), (byte)(blendWhiteHdr.Z / 255f));

                        return blendWhite;
                    }
            }
        }

    }
}