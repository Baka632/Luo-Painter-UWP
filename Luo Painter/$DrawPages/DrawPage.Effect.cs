﻿using Luo_Painter.Brushes;
using Luo_Painter.HSVColorPickers;
using Luo_Painter.Layers;
using Luo_Painter.Options;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Luo_Painter
{
    internal sealed class EdgeDetectionModeButton : ToggleButton
    {
        //@Delegate
        public event RoutedEventHandler Toggled;
        //@Content
        public EdgeDetectionEffectMode Mode { get; private set; }
        //@Construct
        public EdgeDetectionModeButton()
        {
            base.Content = this.Mode;
            base.Unchecked += (s, e) =>
            {
                this.Mode = EdgeDetectionEffectMode.Sobel;
                base.Content = this.Mode;
                this.Toggled?.Invoke(s, e); //Delegate
            };
            base.Checked += (s, e) =>
            {
                this.Mode = EdgeDetectionEffectMode.Prewitt;
                base.Content = this.Mode;
                this.Toggled?.Invoke(s, e); //Delegate
            };
        }
    }

    internal sealed class MorphologyNumberSlider : NumberSliderBase
    {
        //@Delegate
        public event RoutedEventHandler Toggled;
        //@Override
        public override int Number { get; set; } = 1;
        public override int NumberMinimum { get; set; } = -90;
        public override int NumberMaximum { get; set; } = 90;
        //@Content
        public bool IsEmpty { get; private set; } = true;
        public MorphologyEffectMode Mode { get; private set; } = MorphologyEffectMode.Dilate;
        //@Construct
        public MorphologyNumberSlider()
        {
            base.ValueChanged += (s, e) =>
            {
                int size = (int)e.NewValue;
                this.IsEmpty = size == 0;
                this.Mode = size < 0 ? MorphologyEffectMode.Erode : MorphologyEffectMode.Dilate;
                this.Number = Math.Clamp(Math.Abs(size), 1, 90);
                this.Toggled?.Invoke(s, e); //Delegate
                if (this.HeaderButton is null) return;
                this.HeaderButton.Content = size;
            };
        }
    }

    public sealed partial class DrawPage
    {

        public void ConstructEffect()
        {
            // Feather
            this.FeatherSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate
            this.FeatherSlider.Click += (s, e) => this.NumberShowAt(this.FeatherSlider);
            // Grow
            this.GrowSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate
            this.GrowSlider.Click += (s, e) => this.NumberShowAt(this.GrowSlider);
            // Shrink
            this.ShrinkSlider.ValueChanged += (s, e) => this.CanvasControl.Invalidate(); // Invalidate
            this.ShrinkSlider.Click += (s, e) => this.NumberShowAt(this.ShrinkSlider);

            // Exposure
            this.ExposureSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ExposureSlider.Click += (s, e) => this.NumberShowAt(this.ExposureSlider);
            // Brightness
            this.BrightnessSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.BrightnessSlider.Click += (s, e) => this.NumberShowAt(this.BrightnessSlider);
            // Saturation
            this.SaturationSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.SaturationSlider.Click += (s, e) => this.NumberShowAt(this.SaturationSlider);
            // HueRotation
            this.HueRotationSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.HueRotationSlider.Click += (s, e) => this.NumberShowAt(this.HueRotationSlider);
            // Contrast
            this.ContrastSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ContrastSlider.Click += (s, e) => this.NumberShowAt(this.ContrastSlider);
            // Temperature
            this.TemperatureSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.TemperatureSlider.Click += (s, e) => this.NumberShowAt(this.TemperatureSlider, NumberPickerMode.Case0);
            this.TintSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.TintSlider.Click += (s, e) => this.NumberShowAt(this.TintSlider, NumberPickerMode.Case1);
            // HighlightsAndShadows
            this.ShadowsSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowsSlider.Click += (s, e) => this.NumberShowAt(this.ShadowsSlider, NumberPickerMode.Case0);
            this.HighlightsSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.HighlightsSlider.Click += (s, e) => this.NumberShowAt(this.HighlightsSlider, NumberPickerMode.Case1);
            this.ClaritySlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ClaritySlider.Click += (s, e) => this.NumberShowAt(this.ClaritySlider, NumberPickerMode.Case2);
            this.BlurSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.BlurSlider.Click += (s, e) => this.NumberShowAt(this.BlurSlider, NumberPickerMode.Case3);

            // Vignette
            this.VignetteSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.VignetteSlider.Click += (s, e) => this.NumberShowAt(this.VignetteSlider);
            // ColorMatch
            this.ColorMatchSourceButton.SetColor(Colors.Gray);
            this.ColorMatchSourceButton.SetColorHdr(Colors.Gray);
            this.ColorMatchSourceButton.Click += (s, e) => this.ColorShowAt(this.ColorMatchSourceButton, ColorPickerMode.Case0);
            this.ColorMatchDestinationButton.SetColor(Colors.Black);
            this.ColorMatchDestinationButton.SetColorHdr(Vector4.UnitW);
            this.ColorMatchDestinationButton.Click += (s, e) => this.ColorShowAt(this.ColorMatchDestinationButton, ColorPickerMode.Case1);

            // GaussianBlur
            this.GaussianBlurSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.GaussianBlurSlider.Click += (s, e) => this.NumberShowAt(this.GaussianBlurSlider);
            // DirectionalBlur
            this.DirectionalBlurSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.DirectionalBlurSlider.Click += (s, e) => this.NumberShowAt(this.DirectionalBlurSlider, NumberPickerMode.Case0);
            this.DirectionalBlurAngleSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.DirectionalBlurAngleSlider.Click += (s, e) => this.NumberShowAt(this.DirectionalBlurAngleSlider, NumberPickerMode.Case1);
            // Sharpen
            this.SharpenSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.SharpenSlider.Click += (s, e) => this.NumberShowAt(this.SharpenSlider);
            // Shadow
            this.ShadowColorButton.SetColor(Colors.Black);
            this.ShadowColorButton.SetColorHdr(Vector4.UnitW);
            this.ShadowColorButton.Click += (s, e) => this.ColorShowAt(this.ShadowColorButton);
            this.ShadowAmountSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowAmountSlider.Click += (s, e) => this.NumberShowAt(this.ShadowAmountSlider, NumberPickerMode.Case0);
            this.ShadowOpacitySlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowOpacitySlider.Click += (s, e) => this.NumberShowAt(this.ShadowOpacitySlider, NumberPickerMode.Case1);
            this.ShadowXSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowXSlider.Click += (s, e) => this.NumberShowAt(this.ShadowXSlider, NumberPickerMode.Case2);
            this.ShadowYSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ShadowYSlider.Click += (s, e) => this.NumberShowAt(this.ShadowYSlider, NumberPickerMode.Case3);
            // EdgeDetection
            this.EdgeDetectionSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EdgeDetectionSlider.Click += (s, e) => this.NumberShowAt(this.EdgeDetectionSlider, NumberPickerMode.Case0);
            this.EdgeDetectionBlurAmountSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EdgeDetectionBlurAmountSlider.Click += (s, e) => this.NumberShowAt(this.EdgeDetectionBlurAmountSlider, NumberPickerMode.Case1);
            this.EdgeDetectionButton.Unchecked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EdgeDetectionButton.Checked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EdgeDetectionModeButton.Toggled += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            // Morphology
            this.MorphologySlider.Toggled += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            // Emboss
            this.EmbossSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EmbossSlider.Click += (s, e) => this.NumberShowAt(this.EmbossSlider, NumberPickerMode.Case0);
            this.EmbossAngleSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.EmbossAngleSlider.Click += (s, e) => this.NumberShowAt(this.EmbossAngleSlider, NumberPickerMode.Case1);
            // Straighten
            this.StraightenSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.StraightenSlider.Click += (s, e) => this.NumberShowAt(this.StraightenSlider);

            // Posterize
            this.PosterizeSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            // LuminanceToAlpha
            this.LuminanceToAlphaComboBox.SelectionChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            // ChromaKey
            this.ChromaKeyColorButton.SetColor(Colors.Black);
            this.ChromaKeyColorButton.SetColorHdr(Vector4.UnitW);
            this.ChromaKeyColorButton.Click += (s, e) => this.ColorShowAt(this.ChromaKeyColorButton);
            this.ChromaKeyInvertButton.Unchecked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ChromaKeyInvertButton.Checked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ChromaKeyFeatherButton.Unchecked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ChromaKeyFeatherButton.Checked += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ChromaKeySlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ChromaKeySlider.Click += (s, e) => this.NumberShowAt(this.ChromaKeySlider);
            // Border
            this.BorderXComboBox.SelectionChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.BorderYComboBox.SelectionChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            // Colouring
            this.ColouringSlider.ValueChanged += (s, e) => this.CanvasVirtualControl.Invalidate(); // Invalidate
            this.ColouringSlider.Click += (s, e) => this.NumberShowAt(this.ColouringSlider);
            // Tint
            this.TintColorButton.SetColor(Colors.DodgerBlue);
            this.TintColorButton.SetColorHdr(Colors.DodgerBlue);
            this.TintColorButton.Click += (s, e) => this.ColorShowAt(this.TintColorButton);
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
                        Source2 = this.GradientMesh
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

                case OptionType.Threshold:
                    return new PixelShaderEffect(this.ThresholdShaderCodeBytes)
                    {
                        Source2BorderMode = EffectBorderMode.Hard,
                        Source1 = image,
                        Properties =
                        {
                            ["threshold"] = this.Threshold,
                            ["color0"] = this.ThresholdColor0,
                            ["color1"] = this.ThresholdColor1,
                        },
                    };

                case OptionType.Move:
                    return new Transform2DEffect
                    {
                        BorderMode = EffectBorderMode.Hard,
                        InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
                        TransformMatrix = Matrix3x2.CreateTranslation(this.Move),
                        Source = image
                    };
                case OptionType.Transform:
                case OptionType.MarqueeTransform:
                    return new Transform2DEffect
                    {
                        BorderMode = EffectBorderMode.Hard,
                        InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
                        TransformMatrix = this.BoundsMatrix,
                        Source = image
                    };
                case OptionType.FreeTransform:
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
                        Exposure = Math.Clamp((float)this.ExposureSlider.Value / 100, -2, 2),
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
                        Angle = (float)this.HueRotationSlider.Value * MathF.PI / 180,
                        Source = image
                    };
                case OptionType.Contrast:
                    return new ContrastEffect
                    {
                        Contrast = Math.Clamp((float)this.ContrastSlider.Value / 100, -1, 1),
                        Source = image
                    };
                case OptionType.Temperature:
                    return new TemperatureAndTintEffect
                    {
                        Temperature = Math.Clamp((float)this.TemperatureSlider.Value / 100, -1, 1),
                        Tint = Math.Clamp((float)this.TintSlider.Value / 100, -1, 1),
                        Source = image
                    };
                case OptionType.HighlightsAndShadows:
                    return new HighlightsAndShadowsEffect
                    {
                        Shadows = Math.Clamp((float)this.ShadowsSlider.Value / 100, -1, 1),
                        Highlights = Math.Clamp((float)this.HighlightsSlider.Value / 100, -1, 1),
                        Clarity = Math.Clamp((float)this.ClaritySlider.Value / 100, -1, 1),
                        MaskBlurAmount = Math.Clamp((float)this.BlurSlider.Value / 100, 0, 10),
                        Source = image
                    };


                case OptionType.GammaTransfer:
                    return new GammaTransferEffect
                    {
                        AlphaAmplitude = this.GTAA,
                        AlphaExponent = this.GTEA,
                        AlphaOffset = this.GTOA,

                        RedAmplitude = this.GTAR,
                        RedExponent = this.GTER,
                        RedOffset = this.GTOR,

                        GreenAmplitude = this.GTAG,
                        GreenExponent = this.GTEG,
                        GreenOffset = this.GTOG,

                        BlueAmplitude = this.GTAB,
                        BlueExponent = this.GTEB,
                        BlueOffset = this.GTOB,

                        Source = image
                    };
                case OptionType.Vignette:
                    return new VignetteEffect
                    {
                        Color = default,
                        Amount = Math.Clamp((float)this.VignetteSlider.Value / 100, 0, 1),
                        Source = image
                    };
                case OptionType.ColorMatrix:
                    return new ColorMatrixEffect
                    {
                        Source = image
                    };
                case OptionType.ColorMatch:
                    Vector4 sourceHdr = this.ColorMatchSourceButton.ColorHdr;
                    Vector4 destinationHdr = this.ColorMatchDestinationButton.ColorHdr;
                    return new ColorMatrixEffect
                    {
                        Source = image,
                        ColorMatrix = new Matrix5x4
                        {             
                            /// <see cref="AdjustmentExtensions"/>
                            #pragma warning disable IDE0055
                            M11 = sourceHdr.X, M12 = 0, M13 = 0, M14 = 0, // Red
                            M21 = 0, M22 = sourceHdr.Y, M23 = 0, M24 = 0, // Green
                            M31 = 0, M32 = 0, M33 = sourceHdr.Z, M34 = 0, // Blue
                            M41 = 0, M42 = 0, M43 = 0, M44 = sourceHdr.W, // Alpha
                            M51 = destinationHdr.X, M52 = destinationHdr.Y, M53 = destinationHdr.Z, M54 = destinationHdr.W // Offset
                            #pragma warning restore IDE0055
                        }
                    };


                case OptionType.GaussianBlur:
                    return new GaussianBlurEffect
                    {
                        BlurAmount = (float)this.GaussianBlurSlider.Value,
                        Source = image
                    };
                case OptionType.DirectionalBlur:
                    return new DirectionalBlurEffect
                    {
                        BlurAmount = (float)this.DirectionalBlurSlider.Value,
                        Angle = (float)this.DirectionalBlurAngleSlider.Value * MathF.PI / 180,
                        Source = image
                    };
                case OptionType.Sharpen:
                    return new SharpenEffect
                    {
                        Amount = Math.Clamp((float)this.SharpenSlider.Value / 10, 0, 10),
                        //Threshold = Math.Clamp((float)this.SharpenSlider.Value / 10, 0, 10),
                        Source = image
                    };
                case OptionType.Shadow:
                    return new CompositeEffect
                    {
                        Sources =
                        {
                            new OpacityEffect
                            {
                                Opacity = Math.Clamp((float)this.ShadowOpacitySlider.Value/100,0,1),
                                Source = new Transform2DEffect
                                {
                                    TransformMatrix = Matrix3x2.CreateTranslation((float)this.ShadowXSlider.Value, (float)this.ShadowYSlider.Value),
                                    Source = new ShadowEffect
                                    {
                                        BlurAmount = (float)this.ShadowAmountSlider.Value,
                                        ShadowColor = this.ShadowColorButton.Color,
                                        Source = image,
                                    }
                                }
                            },
                            image
                        }
                    };
                case OptionType.EdgeDetection:
                    return new EdgeDetectionEffect
                    {
                        Amount = Math.Clamp((float)this.EdgeDetectionSlider.Value / 100, 0, 1),
                        BlurAmount = Math.Clamp((float)this.EdgeDetectionBlurAmountSlider.Value / 10, 0, 10),
                        OverlayEdges = this.EdgeDetectionButton.IsChecked is true,
                        Mode = this.EdgeDetectionModeButton.Mode,
                        Source = image,
                    };
                case OptionType.Morphology:
                    return this.MorphologySlider.IsEmpty ? image : new MorphologyEffect
                    {
                        Width = this.MorphologySlider.Number,
                        Height = this.MorphologySlider.Number,
                        Mode = this.MorphologySlider.Mode,
                        Source = image,
                    };
                case OptionType.Emboss:
                    return new EmbossEffect
                    {
                        Amount = Math.Clamp((float)this.EmbossSlider.Value / 10, 0, 10),
                        Angle = (float)this.EmbossAngleSlider.Value * MathF.PI / 180,
                        Source = image
                    };
                case OptionType.Straighten:
                    return new StraightenEffect
                    {
                        MaintainSize = true,
                        Angle = (float)this.StraightenSlider.Value * MathF.PI / 180,
                        Source = image
                    };


                case OptionType.Sepia:
                    return new SepiaEffect
                    {
                        Source = image
                    };
                case OptionType.Posterize:
                    int posterize = (int)this.PosterizeSlider.Value;
                    return new PosterizeEffect
                    {
                        RedValueCount = posterize,
                        GreenValueCount = posterize,
                        BlueValueCount = posterize,
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
                case OptionType.ChromaKey:
                    return new ChromaKeyEffect
                    {
                        Tolerance = Math.Clamp((float)this.ChromaKeySlider.Value / 100, 0, 1),
                        InvertAlpha = this.ChromaKeyInvertButton.IsChecked is true,
                        Color = this.ChromaKeyColorButton.Color,
                        Feather = this.ChromaKeyFeatherButton.IsChecked is true,
                        Source = image
                    };
                case OptionType.Border:
                    return new CropEffect
                    {
                        SourceRectangle = this.StartingBorderCrop,
                        Source = new BorderEffect
                        {
                            ExtendX = this.ExtendX,
                            ExtendY = this.ExtendY,
                            Source = new CropEffect
                            {
                                SourceRectangle = this.BorderCrop,
                                Source = image
                            }
                        }
                    };
                case OptionType.Colouring:
                    return new HueRotationEffect
                    {
                        Angle = (float)this.ColouringSlider.Value * MathF.PI / 180,
                        Source = new SepiaEffect
                        {
                            Source = image
                        }
                    };
                case OptionType.Tint:
                    //if (AlphaMaskEffect.IsSupported) { }
                    //if (CrossFadeEffect.IsSupported) { }
                    //if (OpacityEffect.IsSupported) { }
                    if (TintEffect.IsSupported)
                        return new TintEffect
                        {
                            ColorHdr = this.TintColorButton.ColorHdr,
                            Source = image
                        };
                    else
                        return image;
                case OptionType.DiscreteTransfer:
                    return new DiscreteTransferEffect
                    {
                        AlphaTable = this.AlphaTable,
                        RedTable = this.RedTable,
                        GreenTable = this.GreenTable,
                        BlueTable = this.BlueTable,
                        Source = image
                    };

                case OptionType.Lighting:
                    return image;
                //case OptionType.Fog:
                case OptionType.Glass:
                    return image;
                case OptionType.PinchPunch:
                    return image;


                default:
                    return image;
            }
        }

    }
}