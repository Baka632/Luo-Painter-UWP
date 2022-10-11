﻿using FanKit.Transformers;
using Luo_Painter.Blends;
using Luo_Painter.Brushes;
using Luo_Painter.Elements;
using Luo_Painter.Layers;
using Luo_Painter.Layers.Models;
using Luo_Painter.Options;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page, ILayerManager, IInkParameter
    {

        bool StartingToolShow;
        bool StartingLayerShow;
        private bool AntiMistouch => true;

        private void ConstructCanvas()
        {
            this.Canvas.SizeChanged += (s, e) =>
            {
                if (e.NewSize == Size.Empty) return;
                if (e.NewSize == e.PreviousSize) return;

                Vector2 size = this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(e.NewSize.ToVector2());
                this.Transformer.ControlWidth = size.X;
                this.Transformer.ControlHeight = size.Y;

                this.AlignmentGrid.RebuildWithInterpolation(e.NewSize);

                this.CanvasControl.Width =
                this.CanvasAnimatedControl.Width =
                this.CanvasVirtualControl.Width = e.NewSize.Width;

                this.CanvasControl.Height =
                this.CanvasAnimatedControl.Height =
                this.CanvasVirtualControl.Height = e.NewSize.Height;
            };


            this.CanvasControl.Draw += (sender, args) =>
            {
                Matrix3x2 matrix = sender.Dpi.ConvertPixelsToDips(this.Transformer.GetMatrix());
                foreach (ReferenceImage item in this.ReferenceImages)
                {
                    item.Draw(args.DrawingSession, matrix);
                }

                args.DrawingSession.Blend = CanvasBlend.Copy;

                switch (this.OptionType)
                {
                    case OptionType.Feather:
                    case OptionType.MarqueeTransform:
                    case OptionType.Grow:
                    case OptionType.Shrink:
                    case OptionType.SelectionBrush:
                        //@DPI 
                        args.DrawingSession.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                        args.DrawingSession.Transform = this.Transformer.GetMatrix();

                        args.DrawingSession.DrawImage(new OpacityEffect
                        {
                            Opacity = 0.5f,
                            Source = this.GetPreview(this.OptionType, this.Marquee[BitmapType.Source])
                        });

                        switch (this.OptionType)
                        {
                            case OptionType.MarqueeTransform:
                                args.DrawingSession.Transform = Matrix3x2.Identity;
                                args.DrawingSession.Units = CanvasUnits.Dips; /// <see cref="DPIExtensions">

                                this.DrawTransform(sender, args.DrawingSession, matrix);
                                break;
                        }
                        break;

                    case OptionType.CropCanvas:
                        this.DrawCropCanvas(sender, args.DrawingSession);
                        break;

                    case OptionType.Transform:
                        this.DrawTransform(sender, args.DrawingSession, matrix);
                        break;
                    case OptionType.DisplacementLiquefaction:
                        this.DrawDisplacementLiquefaction(sender, args.DrawingSession);
                        break;
                    case OptionType.GradientMapping:
                        break;
                    case OptionType.RippleEffect:
                        this.DrawRippleEffect(sender, args.DrawingSession);
                        break;

                    default:
                        if (this.OptionType.IsGeometry())
                        {
                            if (this.BitmapLayer is null) break;

                            args.DrawingSession.DrawBoundNodes(this.BoundsTransformer, matrix);
                            break;
                        }

                        if (this.OptionType.IsMarquee())
                        {
                            args.DrawingSession.DrawMarqueeTool(this.CanvasDevice, this.MarqueeToolType, this.MarqueeTool, sender.Dpi.ConvertPixelsToDips(this.Transformer.GetMatrix()));
                            break;
                        }

                        if (this.OptionType.IsGeometry())
                        {
                            if (this.BitmapLayer is null) break;

                            args.DrawingSession.DrawBound(this.BoundsTransformer, matrix);
                        }
                        break;
                }
            };


            this.CanvasAnimatedControl.CreateResources += (sender, args) =>
            {
                this.CreateMarqueeResources(this.Transformer.Width, this.Transformer.Height);
                args.TrackAsyncAction(this.CreateDottedLineResourcesAsync().AsAsyncAction());
            };
            this.CanvasAnimatedControl.Draw += (sender, args) =>
            {
                //@DPI 
                args.DrawingSession.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">
                args.DrawingSession.Blend = CanvasBlend.Copy;

                args.DrawingSession.DrawImage(new PixelShaderEffect(this.DottedLineTransformShaderCodeBytes)
                {
                    Source1 = this.Marquee[BitmapType.Source],
                    Properties =
                    {
                        ["time"] = (float)args.Timing.UpdateCount,
                        ["lineWidth"] = sender.Dpi.ConvertDipsToPixels(2),
                        ["left"] = 0f,
                        ["top"] = 0f,
                        ["right"] = (float)this.Transformer.Width,
                        ["bottom"] = (float)this.Transformer.Height,
                        ["matrix3x2"] = this.Transformer.GetInverseMatrix(),
                    },
                });
            };


            this.CanvasVirtualControl.CreateResources += (sender, args) =>
            {
                this.GradientMesh = new GradientMesh(this.CanvasDevice);
                this.GrayAndWhiteMesh = CanvasBitmap.CreateFromColors(this.CanvasDevice, new Color[]
                {
                    Colors.LightGray, Colors.White,
                    Colors.White, Colors.LightGray
                }, 2, 2);
                this.CreateResources(this.Transformer.Width, this.Transformer.Height);
                args.TrackAsyncAction(this.CreateResourcesAsync().AsAsyncAction());
            };
            this.CanvasVirtualControl.RegionsInvalidated += (sender, args) =>
            {
                foreach (Rect region in args.InvalidatedRegions)
                {
                    using (CanvasDrawingSession ds = sender.CreateDrawingSession(region))
                    using (Transform2DEffect mesh = new Transform2DEffect
                    {
                        Source = this.Mesh[BitmapType.Source],
                        TransformMatrix = this.Transformer.GetMatrix(),
                        InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
                    })
                    {
                        //@DPI 
                        ds.Units = CanvasUnits.Pixels; /// <see cref="DPIExtensions">

                        // Mesh
                        // Layer
                        if (this.OptionType.IsEdit() || this.BitmapLayer is null)
                            ds.DrawImage(this.Nodes.Render(mesh, this.Transformer.GetMatrix(), CanvasImageInterpolation.NearestNeighbor));
                        else
                            ds.DrawImage(this.Nodes.Render(mesh, this.Transformer.GetMatrix(), CanvasImageInterpolation.NearestNeighbor, this.BitmapLayer.Id, this.GetMezzanine()));
                    }
                }
            };
        }

        private ICanvasImage GetMezzanine()
        {
            if (this.OptionType.IsGeometry())
            {
                return new CompositeEffect { Sources = { this.BitmapLayer[BitmapType.Source], this.BitmapLayer[BitmapType.Temp] } };
            }

            if (this.OptionType.IsEffect() is false)
            {
                switch (this.OptionType)
                {
                    case OptionType.Brush:
                        return this.GetBrushPreview();
                    case OptionType.Transparency:
                        return this.GetTransparencyPreview();
                    default:
                        if (this.OptionType.HasFlag(OptionType.Geometry))
                        {
                            return new CompositeEffect { Sources = { this.BitmapLayer[BitmapType.Source], this.BitmapLayer[BitmapType.Temp] } };
                        }
                        else
                        {
                            return this.InkPresenter.GetPreview(this.InkType, this.BitmapLayer[BitmapType.Source], this.BitmapLayer[BitmapType.Temp]);
                        }
                }
            }


            if (this.SelectionType is SelectionType.MarqueePixelBounds is false)
            {
                return this.GetPreview(this.OptionType, this.BitmapLayer[BitmapType.Source]);
            }


            if (this.OptionType.HasDifference())
            {
                return new CompositeEffect
                {
                    Sources =
                    {
                        new PixelShaderEffect(this.RalphaMaskShaderCodeBytes)
                        {
                            Source1 = this.Marquee[BitmapType.Source],
                            Source2 = this.BitmapLayer[BitmapType.Source],
                        },
                        this.GetPreview(this.OptionType, new AlphaMaskEffect
                        {
                            AlphaMask = this.Marquee[BitmapType.Source],
                            Source = this.BitmapLayer[BitmapType.Source]
                        })
                    }
                };
            }
            else
            {
                return new PixelShaderEffect(this.LalphaMaskShaderCodeBytes)
                {
                    Source1 = this.Marquee[BitmapType.Source],
                    Source2 = this.BitmapLayer[BitmapType.Origin],
                    Source3 = this.GetPreview(this.OptionType, this.BitmapLayer[BitmapType.Origin])
                };
            }
        }

    }
}