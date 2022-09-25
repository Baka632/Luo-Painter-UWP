﻿using Luo_Painter.Layers.Models;
using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.UI;

namespace Luo_Painter.Layers
{
    public static class CanvasDrawingSessionExtensions
    {

        private static void DrawAnchor(this CanvasDrawingSession ds, Anchor anchor, Vector2 point)
        {
            if (anchor.IsSmooth)
            {
                if (anchor.IsChecked) ds.FillCircle(point, 4, Colors.White);
                ds.FillCircle(point, 3, Colors.DodgerBlue);
            }
            else
            {
                if (anchor.IsChecked) ds.FillRectangle(point.X - 3, point.Y - 3, 6, 6, Colors.White);
                ds.FillRectangle(point.X - 2, point.Y - 2, 4, 4, Colors.DodgerBlue);
            }
        }

        public static void DrawAnchorCollection(this CanvasDrawingSession ds, AnchorCollection anchors)
        {
            foreach (Anchor item in anchors)
            {
                ds.DrawAnchor(item, item.Point);
            }
        }
        public static void DrawAnchorCollection(this CanvasDrawingSession ds, AnchorCollection anchors, Matrix3x2 matrix)
        {
            foreach (Anchor item in anchors)
            {
                ds.DrawAnchor(item, Vector2.Transform(item.Point, matrix));
            }
        }


        private static void FillAnchor(this CanvasDrawingSession ds, Anchor anchor, Color color, float strokeWidth)
        {
            float sw = anchor.Pressure * strokeWidth / 2;
            float swHalf = sw / 2;

            // 1.Head
            ds.FillCircle(anchor.Point, sw, color);

            // 2.Body
            for (float i = swHalf; i < anchor.ComputePathLength; i += swHalf)
            {
                ds.FillCircle(anchor.Geometry.ComputePointOnPath(i), sw, color);
            }
        }
        private static void FillAnchor(this CanvasDrawingSession ds, Anchor anchor, float pressure, Color color, float strokeWidth)
        {
            // 1.Head
            float sw2 = System.Math.Max(0.4f, anchor.Pressure * strokeWidth / 2);

            ds.FillCircle(anchor.Point, sw2, color);
            float i = sw2 / 2;

            // 2.Body
            do
            {
                float pect = i / anchor.ComputePathLength;
                float sw = System.Math.Max(0.4f, (pect * pressure + (1 - pect) * anchor.Pressure) * strokeWidth / 2);

                ds.FillCircle(anchor.Geometry.ComputePointOnPath(i), sw, color);
                i += sw / 2;
            } while (i < anchor.ComputePathLength);
        }

        /// <summary>
        /// (Head+Body + Head+Body + Head+Body + ... + Foot)
        /// </summary>
        public static void FillAnchorCollection(this CanvasDrawingSession ds, AnchorCollection anchors, Color color, float strokeWidth)
        {
            Anchor previous = null;

            // 1.Head
            // 2.Body
            foreach (Anchor item in anchors)
            {
                if (previous is null)
                {
                    previous = item;
                    continue;
                }

                if (previous.Geometry is null is false)
                {
                    if (previous.Pressure == item.Pressure)
                        ds.FillAnchor(previous, color, strokeWidth);
                    else
                        ds.FillAnchor(previous, item.Pressure, color, strokeWidth);
                }
                previous = item;
            }

            // 3.Foot
            ds.FillCircle(previous.Point, System.Math.Max(0.4f, previous.Pressure * strokeWidth / 2), color);
        }
        /// <summary>
        /// (Head+Body + Head+Body + Head+Body + ... + Head+Body+Foot)
        /// </summary>
        public static void FillAnchorCollection(this CanvasDrawingSession ds, AnchorCollection anchors, Vector2 point, Color color, float strokeWidth)
        {
            Anchor previous = null;

            // 1.Head
            // 2.Body
            foreach (Anchor item in anchors)
            {
                if (previous is null)
                {
                    previous = item;
                    continue;
                }

                if (previous.Geometry is null is false)
                {
                    if (previous.Pressure == item.Pressure)
                        ds.FillAnchor(previous, color, strokeWidth);
                    else
                        ds.FillAnchor(previous, item.Pressure, color, strokeWidth);
                }
                previous = item;
            }

            // 1.Head
            // 2.Body
            if (previous.Geometry is null is false)
            {
                if (previous.Pressure == 1)
                    ds.FillAnchor(previous, color, strokeWidth);
                else
                    ds.FillAnchor(previous, 1, color, strokeWidth);
            }

            // 3.Foot
            ds.FillCircle(point, System.Math.Max(0.4f, strokeWidth / 2), color);
        }

    }
}