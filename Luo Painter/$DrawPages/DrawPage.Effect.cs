﻿using Luo_Painter.Brushes;
using Luo_Painter.Layers;
using Luo_Painter.Options;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page, ILayerManager, IInkParameter
    {

        public void ConstructEffect()
        {
            this.FeatherSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate
            this.GrowSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate
            this.ShrinkSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate

            this.ExposureSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.BrightnessSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.SaturationSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.HueRotationSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ContrastSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.TemperatureSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.TintSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowsSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.HighlightsSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ClaritySlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.BlurSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate

            this.LuminanceToAlphaComboBox.SelectionChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
        }

        private ICanvasImage GetPreview(OptionType type, ICanvasImage image)
        {
            switch (type)
            {
                case OptionType.None:
                    return image;


                case OptionType.DisplacementLiquefaction:
                    return new DisplacementMapEffect
                    {
                        XChannelSelect = EffectChannelSelect.Red,
                        YChannelSelect = EffectChannelSelect.Green,
                        Amount = this.DisplacementLiquefactionAmount,
                        Source = image,
                        Displacement = new GaussianBlurEffect
                        {
                            BlurAmount = 16,
                            Source = new BorderEffect
                            {
                                ExtendX = CanvasEdgeBehavior.Clamp,
                                ExtendY = CanvasEdgeBehavior.Clamp,
                                Source = this.Displacement[BitmapType.Source],
                            }
                        }
                    };

                case OptionType.GradientMapping:
                    return new PixelShaderEffect(this.GradientMappingShaderCodeBytes)
                    {
                        Source2BorderMode = EffectBorderMode.Hard,
                        Source1 = image,
                        Source2 = this.GradientMesh.Source
                    };

                case OptionType.RippleEffect:
                    return new PixelShaderEffect(this.RippleEffectShaderCodeBytes)
                    {
                        Source2BorderMode = EffectBorderMode.Hard,
                        Source1 = image,
                        Properties =
                        {
                            ["frequency"] = this.Rippler.Frequency,
                            ["phase"] = this.Rippler.Phase,
                            ["amplitude"] = this.Rippler.Amplitude,
                            ["spread"] = this.Rippler.Spread,
                            ["center"] = this.RipplerCenter,
                            ["dpi"] = 96.0f, // Default value 96f,
                        },
                    };

                case OptionType.Transform:
                case OptionType.MarqueeTransform:
                    switch (this.TransformMode)
                    {
                        case 0:
                            return new Transform2DEffect
                            {
                                BorderMode = EffectBorderMode.Hard,
                                InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
                                TransformMatrix = Matrix3x2.CreateTranslation(this.Move),
                                Source = image
                            };
                        case 1:
                            return new Transform2DEffect
                            {
                                BorderMode = EffectBorderMode.Hard,
                                InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
                                TransformMatrix = this.BoundsMatrix,
                                Source = image
                            };
                        case 2:
                            return new PixelShaderEffect(this.FreeTransformShaderCodeBytes)
                            {
                                Source1 = image,
                                Properties =
                                {
                                    ["matrix3x2"] = this.BoundsFreeMatrix,
                                    ["zdistance"] = this.BoundsFreeDistance,
                                    ["left"] = this.Bounds.Left,
                                    ["top"] = this.Bounds.Top,
                                    ["right"] = this.Bounds.Right,
                                    ["bottom"] = this.Bounds.Bottom,
                                },
                            };
                        default:
                            return image;
                    }


                case OptionType.Feather:
                    return new GaussianBlurEffect
                    {
                        BlurAmount = (float)this.FeatherSlider.Value,
                        Source = image
                    };
                case OptionType.Grow:
                    int grow = (int)this.GrowSlider.Value;
                    return new MorphologyEffect
                    {
                        Mode = MorphologyEffectMode.Dilate,
                        Height = grow,
                        Width = grow,
                        Source = image
                    };
                case OptionType.Shrink:
                    int shrink = (int)this.ShrinkSlider.Value;
                    return new MorphologyEffect
                    {
                        Mode = MorphologyEffectMode.Erode,
                        Height = shrink,
                        Width = shrink,
                        Source = image
                    };


                case OptionType.Gray:
                    return new GrayscaleEffect
                    {
                        Source = image
                    };
                case OptionType.Invert:
                    return new InvertEffect
                    {
                        Source = image
                    };
                case OptionType.Exposure:
                    return new ExposureEffect
                    {
                        Exposure = (float)this.ExposureSlider.Value / 100,
                        Source = image
                    };
                case OptionType.Brightness:
                    float brightness = (float)this.BrightnessSlider.Value / 100;
                    return new BrightnessEffect
                    {
                        WhitePoint = new Vector2(System.Math.Clamp(2 - brightness, 0, 1), 1),
                        BlackPoint = new Vector2(System.Math.Clamp(1 - brightness, 0, 1), 0),
                        Source = image
                    };
                case OptionType.Saturation:
                    return new SaturationEffect
                    {
                        Saturation = (float)this.SaturationSlider.Value / 100,
                        Source = image
                    };
                case OptionType.HueRotation:
                    return new HueRotationEffect
                    {
                        Angle = (float)this.HueRotationSlider.Value / 180 * FanKit.Math.Pi,
                        Source = image
                    };
                case OptionType.Contrast:
                    return new ContrastEffect
                    {
                        Contrast = (float)this.ContrastSlider.Value / 100,
                        Source = image
                    };
                case OptionType.Temperature:
                    return new TemperatureAndTintEffect
                    {
                        Temperature = (float)this.TemperatureSlider.Value / 100,
                        Tint = (float)this.TintSlider.Value / 100,
                        Source = image
                    };
                case OptionType.HighlightsAndShadows:
                    return new HighlightsAndShadowsEffect
                    {
                        Shadows = (float)this.ShadowsSlider.Value / 100,
                        Highlights = (float)this.HighlightsSlider.Value / 100,
                        Clarity = (float)this.ClaritySlider.Value / 100,
                        MaskBlurAmount = (float)this.BlurSlider.Value / 100,
                        Source = image
                    };

                case OptionType.LuminanceToAlpha:
                    switch (this.LuminanceToAlphaComboBox.SelectedIndex)
                    {
                        case 0:
                            return new LuminanceToAlphaEffect
                            {
                                Source = image
                            };
                        case 1:
                            return new LuminanceToAlphaEffect
                            {
                                Source = new InvertEffect
                                {
                                    Source = image
                                }
                            };
                        case 2:
                            return new InvertEffect
                            {
                                Source = new LuminanceToAlphaEffect
                                {
                                    Source = new InvertEffect
                                    {
                                        Source = image
                                    }
                                }
                            };
                        default: return image;
                    }
                //case OptionType.Fog:
                case OptionType.Sepia:
                    return new SepiaEffect
                    {
                        Source = image
                    };
                case OptionType.Posterize:
                    return new PosterizeEffect
                    {
                        Source = image
                    };

                default:
                    return image;
            }
        }

    }
}