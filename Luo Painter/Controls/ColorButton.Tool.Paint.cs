﻿using Luo_Painter.Brushes;
using Luo_Painter.Layers;
using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;

namespace Luo_Painter.Controls
{
    public sealed partial class ColorButton
    {
        private void PaintStop()
        {
            this.Tasks.State = PaintTaskState.Painted;
        }
        private async void PaintStart()
        {
            this.Tasks.State = PaintTaskState.Painting;
            await Task.Run(this.PaintAction);
        }

        private void PaintAction()
        {
            while (true)
            {
                switch (this.Tasks.GetBehavior())
                {
                    case PaintTaskBehavior.WaitingWork:
                        continue;
                    case PaintTaskBehavior.Working:
                    case PaintTaskBehavior.WorkingBeforeDead:
                        StrokeSegment segment = this.Tasks[0];
                        this.Tasks.RemoveAt(0);
                        this.PaintDelta(segment);
                        break;
                    default:
                        //@Paint
                        this.Tasks.State = PaintTaskState.Finished;
                        this.Tasks.Clear();
                        this.PaintCompleted();
                        return;
                }
            }
        }

        private void PaintCapAsync(StrokeCap cap)
        {
            this.PaintStarted(cap);
        }
        private void PaintStarted(StrokeCap cap)
        {
            //@Task
            lock (this.Locker)
            {
                this.BitmapLayer.Hit(cap.Bounds);
                this.PaintCap(cap);
            }

            this.CanvasControl.Invalidate(); // Invalidate
        }

        private async void PaintSegmentAsync()
        {
            this.PaintAction();
        }
        private async void PaintDelta(StrokeSegment segment)
        {
            //@Task
            lock (this.Locker)
            {
                this.BitmapLayer.Hit(segment.Bounds);
                this.PaintSegment(segment);
            }

            await this.CanvasControl.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                this.CanvasControl.Invalidate();
            });
        }
        private async void PaintCompleted()
        {
            //@Task
            lock (this.Locker)
            {
                if (this.InkType is InkType.Liquefy is false)
                {
                    using (CanvasDrawingSession ds = this.BitmapLayer.CreateDrawingSession())
                    {
                        ds.Clear(Colors.Transparent);
                        this.InkPresenter.Preview(ds, this.InkType, this.BitmapLayer[BitmapType.Origin], this.BitmapLayer[BitmapType.Temp]);
                    }
                }
                this.BitmapLayer.Clear(Colors.Transparent, BitmapType.Temp);
            }

            await this.CanvasControl.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                // History
                this.BitmapLayer.Flush();
                this.BitmapLayer.RenderThumbnail();

                this.CanvasControl.Invalidate(); // Invalidate
            });
        }

    }
}