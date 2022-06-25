﻿using FanKit.Transformers;
using Luo_Painter.Elements;
using Luo_Painter.Historys.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Linq;
using System.Numerics;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page
    {

        TransformerMode CropMode;
        bool IsCropMove;

        Transformer CropTransformer;
        Transformer StartingCropTransformer;


        private Vector2 ToPositionWithoutRadian(Vector2 point) => Vector2.Transform(this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(point),
                Matrix3x2.CreateTranslation(-this.Transformer.Position) *
                Matrix3x2.CreateScale(1 / this.Transformer.Scale) *
                Matrix3x2.CreateTranslation(this.Transformer.Width / 2, this.Transformer.Height / 2));
        private Matrix3x2 GetMatrixWithoutRadian(float dpi) => dpi.ConvertPixelsToDips(
                Matrix3x2.CreateTranslation(-this.Transformer.Width / 2, -this.Transformer.Height / 2) *
                Matrix3x2.CreateScale(this.Transformer.Scale) *
                Matrix3x2.CreateTranslation(this.Transformer.Position));

        private Matrix3x2 GetMatrix(Vector2 leftTop) =>
            Matrix3x2.CreateRotation(this.Transformer.Radian,
                new Vector2(this.Transformer.Width / 2, this.Transformer.Height / 2)) *
            Matrix3x2.CreateTranslation(-leftTop);

        private void SetCropCanvas(int w, int h)
        {
            this.CropTransformer = new Transformer(0, 0, w, h);

            this.CropCanvasSlider.Value = 0d;

            if (this.Transformer.Radian == 0f) return;
            this.Transformer.Radian = 0f;
            this.Transformer.ReloadMatrix();
        }

        private void DrawCropCanvas(CanvasControl sender, CanvasDrawingSession ds)
        {
            Transformer crop = FanKit.Transformers.Transformer.Multiplies(this.CropTransformer, this.GetMatrixWithoutRadian(sender.Dpi));

            ds.Clear(CanvasDrawingSessionExtensions.ShadowColor);
            ds.FillRectangle(crop.MinX, crop.MinY, crop.MaxX - crop.MinX, crop.MaxY - crop.MinY, Colors.Transparent);

            ds.DrawCrop(crop);
        }


        /// <summary> <see cref="Transform_Start(Vector2)"/> </summary>
        private void CropCanvas_Start(Vector2 point)
        {
            this.StartingPosition = this.ToPositionWithoutRadian(point);

            this.CropMode = FanKit.Transformers.Transformer.ContainsNodeMode(point, this.CropTransformer, this.GetMatrixWithoutRadian(this.CanvasVirtualControl.Dpi), true);
            this.IsCropMove = this.CropMode == TransformerMode.None;
            this.StartingCropTransformer = this.CropTransformer;
        }

        /// <summary> <see cref="Transform_Delta(Vector2)"/> </summary>
        private void CropCanvas_Delta(Vector2 point)
        {
            Vector2 position = this.ToPositionWithoutRadian(point);

            this.CropTransformer =
                this.IsCropMove ?
                this.StartingCropTransformer + (position - this.StartingPosition) :
                FanKit.Transformers.Transformer.Controller(this.CropMode, this.StartingPosition, position, this.StartingCropTransformer);

            this.CanvasControl.Invalidate(); // Invalidate
        }

        /// <summary> <see cref="Transform_Complete(Vector2)"/> </summary>
        private void CropCanvas_Complete(Vector2 point)
        {
        }


        private void CancelCropCanvas()
        {
            if (this.CropCanvasSlider.Value is 0d) return;

            this.Transformer.Radian = 0f;
            this.Transformer.ReloadMatrix();
            this.CropCanvasSlider.Value = 0;
        }
        private void PrimaryCropCanvas()
        {
            int width2 = this.Transformer.Width;
            int height2 = this.Transformer.Height;

            uint width = (uint)width2;
            uint height = (uint)height2;

            float w3 = this.CropTransformer.MaxX - this.CropTransformer.MinX;
            float h3 = this.CropTransformer.MaxY - this.CropTransformer.MinY;

            int w2 = (int)w3;
            int h2 = (int)h3;

            uint w = (uint)w2;
            uint h = (uint)h2;

            Vector2 offset = new Vector2
            {
                X = -this.CropTransformer.MinX,
                Y = -this.CropTransformer.MinY,
            };

            if (this.CropCanvasSlider.Value is 0d)
            {
                this.Setup(w2, h2);
                this.Setup(this.ObservableCollection.Select(c => c.Crop(this.CanvasDevice, w2, h2, offset)).ToArray(), new SetupSizes
                {
                    UndoParameter = new BitmapSize { Width = width, Height = height },
                    RedoParameter = new BitmapSize { Width = w, Height = h }
                });
            }
            else
            {
                Matrix3x2 matrix = this.GetMatrix(this.CropTransformer.LeftTop);

                this.Setup(w2, h2);
                this.Setup(this.ObservableCollection.Select(c => c.Crop(this.CanvasDevice, w2, h2, matrix, CanvasImageInterpolation.NearestNeighbor)).ToArray(), new SetupSizes
                {
                    UndoParameter = new BitmapSize
                    {
                        Width = width,
                        Height = height
                    },
                    RedoParameter = new BitmapSize
                    {
                        Width = w,
                        Height = h
                    }
                });

                this.Transformer.Radian = 0f;
                this.Transformer.ReloadMatrix();
                this.CropCanvasSlider.Value = 0;
            }
        }

    }
}