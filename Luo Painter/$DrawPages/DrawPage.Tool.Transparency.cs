﻿using Luo_Painter.Blends;
using Luo_Painter.Brushes;
using Luo_Painter.Layers;
using Luo_Painter.Layers.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page, ILayerManager, IInkParameter
    {

        private Color GetStartColor()
        {
            switch (this.TransparencyReverseCheckBox.IsChecked)
            {
                case true: return Colors.Transparent;
                default: return Colors.Black;
            }
        }
        private Color GetEndColor()
        {
            switch (this.TransparencyReverseCheckBox.IsChecked)
            {
                case true: return Colors.Black;
                default: return Colors.Transparent;
            }
        }

        private ICanvasImage GetTransparencyPreview()
        {
            switch (this.SelectionType)
            {
                case SelectionType.None:
                    return this.BitmapLayer[BitmapType.Source];
                case SelectionType.All:
                    return new PixelShaderEffect(this.RalphaMaskEffectShaderCodeBytes)
                    {
                        Source1 = this.BitmapLayer[BitmapType.Temp],
                        Source2 = this.BitmapLayer[BitmapType.Source]
                    };
                case SelectionType.PixelBounds:
                case SelectionType.MarqueePixelBounds:
                    return new PixelShaderEffect(this.LalphaMaskEffectShaderCodeBytes)
                    {
                        Source1 = this.Marquee[BitmapType.Source],
                        Source2 = this.BitmapLayer[BitmapType.Source],
                        Source3 = this.BitmapLayer[BitmapType.Temp]
                    };
                default:
                    return this.BitmapLayer[BitmapType.Source];
            }
        }


        private void Transparency_Start(Vector2 position, Vector2 point)
        {
            this.BitmapLayer = this.LayerSelectedItem as BitmapLayer;
            if (this.BitmapLayer is null)
            {
                this.Tip(TipType.NoLayer);
                return;
            }

            this.SelectionType = this.BitmapLayer.GetDrawSelection(false, this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
            switch (this.SelectionType)
            {
                case SelectionType.None:
                    this.Tip(TipType.NoPixelForBitmapLayer);
                    return;
                case SelectionType.All:
                    break;
                default:
                    this.BitmapLayer.Hit(InterpolationColors);
                    this.BrushBounds = this.Marquee.CreateInterpolationBoundsScaled(InterpolationColors).ToRect();
                    break;
            }

            switch (this.AppBar.TransparencyMode)
            {
                case 0:
                    this.LinearGradientBrush = new CanvasLinearGradientBrush(this.CanvasDevice, this.GetStartColor(), this.GetEndColor())
                    {
                        StartPoint = position,
                        EndPoint = position,
                    };
                    break;
                case 1:
                case 2:
                    this.RadialGradientBrush = new CanvasRadialGradientBrush(this.CanvasDevice, this.GetStartColor(), this.GetEndColor())
                    {
                        Center = position,
                        RadiusX = 10,
                        RadiusY = 10,
                    };
                    break;
                default:
                    break;
            }

            this.CanvasVirtualControl.Invalidate(); // Invalidate
        }

        private void Transparency_Delta(Vector2 position, Vector2 point)
        {
            if (this.BitmapLayer is null) return;
            if (this.SelectionType is SelectionType.None) return;
            if (Vector2.DistanceSquared(this.StartingPoint, point) < 100) return;

            switch (this.AppBar.TransparencyMode)
            {
                case 0:
                    this.LinearGradientBrush.EndPoint = position;
                    this.TransparencyClear(this.LinearGradientBrush);
                    break;
                case 1:
                    this.RadialGradientBrush.RadiusX =
                    this.RadialGradientBrush.RadiusY =
                    Vector2.Distance(this.StartingPosition, position);
                    this.TransparencyClear(this.RadialGradientBrush);
                    break;
                case 2:
                    this.RadialGradientBrush.RadiusX = System.Math.Abs(this.StartingPosition.X - position.X);
                    this.RadialGradientBrush.RadiusY = System.Math.Abs(this.StartingPosition.Y - position.Y);
                    this.TransparencyClear(this.RadialGradientBrush);
                    break;
                default:
                    break;
            }

            this.CanvasVirtualControl.Invalidate(); // Invalidate
        }

        private void Transparency_Complete(Vector2 position, Vector2 point)
        {
            if (this.BitmapLayer is null) return;
            if (this.SelectionType is SelectionType.None) return;

            switch (this.AppBar.TransparencyMode)
            {
                case 0:
                    this.LinearGradientBrush?.Dispose();
                    this.Transparency(new CanvasLinearGradientBrush(this.CanvasDevice, this.GetStartColor(), this.GetEndColor())
                    {
                        StartPoint = this.StartingPosition,
                        EndPoint = position
                    });
                    break;
                case 1:
                    float radius = Vector2.Distance(this.StartingPosition, position);
                    this.RadialGradientBrush?.Dispose();
                    this.Transparency(new CanvasRadialGradientBrush(this.CanvasDevice, this.GetStartColor(), this.GetEndColor())
                    {
                        Center = this.StartingPosition,
                        RadiusX = radius,
                        RadiusY = radius
                    });
                    break;
                case 2:
                    this.RadialGradientBrush?.Dispose();
                    this.Transparency(new CanvasRadialGradientBrush(this.CanvasDevice, this.GetStartColor(), this.GetEndColor())
                    {
                        Center = this.StartingPosition,
                        RadiusX = System.Math.Abs(this.StartingPosition.X - position.X),
                        RadiusY = System.Math.Abs(this.StartingPosition.Y - position.Y)
                    });
                    break;
                default:
                    break;
            }

            this.SelectionType = default;

            this.BitmapLayer.Flush();
            this.BitmapLayer.Clear(Colors.Transparent, BitmapType.Temp);
            this.BitmapLayer.RenderThumbnail();
            this.BitmapLayer = null;

            this.CanvasVirtualControl.Invalidate(); // Invalidate

            this.UndoButton.IsEnabled = this.History.CanUndo;
            this.RedoButton.IsEnabled = this.History.CanRedo;
        }


        private void TransparencyClear(ICanvasBrush brush)
        {
            switch (this.SelectionType)
            {
                case SelectionType.None:
                    return;
                case SelectionType.All:
                case SelectionType.PixelBounds:
                    this.BitmapLayer.Clear(brush, BitmapType.Temp);
                    break;
                case SelectionType.MarqueePixelBounds:
                    this.BitmapLayer.Clear(brush, this.BrushBounds, BitmapType.Temp);
                    break;
                default:
                    break;
            }
        }

        private void Transparency(ICanvasBrush brush)
        {
            switch (this.SelectionType)
            {
                case SelectionType.None:
                    break;
                case SelectionType.All:
                    {
                        using (CanvasCommandList commandList = new CanvasCommandList(this.CanvasDevice))
                        {
                            using (CanvasDrawingSession ds = commandList.CreateDrawingSession())
                            {
                                ds.FillRectangle(0, 0, this.BitmapLayer.Width, this.BitmapLayer.Height, brush);
                            }
                            this.BitmapLayer.DrawCopy(new PixelShaderEffect(this.RalphaMaskEffectShaderCodeBytes)
                            {
                                Source1 = commandList,
                                Source2 = this.BitmapLayer[BitmapType.Origin]
                            }, BitmapType.Source);
                        }

                        // History
                        int removes = this.History.Push(this.BitmapLayer.GetBitmapResetHistory());
                    }
                    break;
                case SelectionType.PixelBounds:
                    {
                        using (CanvasCommandList commandList = new CanvasCommandList(this.CanvasDevice))
                        {
                            using (CanvasDrawingSession ds = commandList.CreateDrawingSession())
                            {
                                ds.FillRectangle(this.BrushBounds, brush);
                            }
                            this.BitmapLayer.DrawCopy(new PixelShaderEffect(this.RalphaMaskEffectShaderCodeBytes)
                            {
                                Source1 = commandList,
                                Source2 = this.BitmapLayer[BitmapType.Origin]
                            }, this.BrushBounds, BitmapType.Source);
                        }

                        // History
                        int removes = this.History.Push(this.BitmapLayer.GetBitmapResetHistory());
                    }
                    break;
                case SelectionType.MarqueePixelBounds:
                    {
                        using (CanvasCommandList commandList = new CanvasCommandList(this.CanvasDevice))
                        {
                            using (CanvasDrawingSession ds = commandList.CreateDrawingSession())
                            {
                                ds.FillRectangle(this.BrushBounds, brush);
                            }
                            this.BitmapLayer.DrawCopy(new PixelShaderEffect(this.LalphaMaskEffectShaderCodeBytes)
                            {
                                Source1 = this.Marquee[BitmapType.Source],
                                Source2 = this.BitmapLayer[BitmapType.Origin],
                                Source3 = commandList
                            }, this.BrushBounds, BitmapType.Source);
                        }

                        // History
                        int removes = this.History.Push(this.BitmapLayer.GetBitmapHistory());
                    }
                    break;
                default:
                    break;
            }
        }

    }
}