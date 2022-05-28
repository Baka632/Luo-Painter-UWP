﻿using Luo_Painter.Historys;
using Luo_Painter.Historys.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.ComponentModel;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Luo_Painter.Layers
{
    public abstract partial class LayerBase
    {

        public string Id { get; } = Guid.NewGuid().ToString();

        public readonly Vector2 Center; // (125, 125)
        public readonly int Width; // 250
        public readonly int Height; // 250

        public LayerBase(ICanvasResourceCreator resourceCreator, int width, int height)
        {
            this.Center = new Vector2((float)width / 2, (float)height / 2);
            this.Width = width;
            this.Height = height;


            this.ThumbnailWriteableBitmap = new WriteableBitmap(50, 50);
            this.ThumbnailRenderTarget = new CanvasRenderTarget(resourceCreator, 50, 50, 96);

            int wh = System.Math.Max(width, height);
            if (wh <= 50) this.ThumbnailType = ThumbnailType.Origin;
            else if (wh >= 5000) this.ThumbnailType = ThumbnailType.Oversize;
            else
            {
                this.ThumbnailScale = new Vector2(50f / wh);
                this.ThumbnailType = ThumbnailType.None;
            }
        }


        //@Notify 
        /// <summary> Multicast event for property change notifications. </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName"> Name of the property used to notify listeners. </param>
        protected void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}