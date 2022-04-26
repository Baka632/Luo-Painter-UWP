﻿using Luo_Painter.Shaders;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Luo_Painter.TestApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MagicWandPage : Page
    {
        CanvasBitmap originalImage;
        CanvasBitmap effectImage;
        Color[] originalColors;
        Color[] effectColors;
        ScaleEffect se;
        int imageWidth;
        int imageHeight;

        byte[] dottedLineCode;
        PixelShaderEffect pse;
        CanvasRenderTarget crt;
        /// <summary>
        /// 用于记录已近检索的像素
        /// </summary>
        Dictionary<Pos, int> tempDir = new Dictionary<Pos, int>();
        DispatcherTimer timer = new DispatcherTimer();

        Vector2 offset = new Vector2();
        float scale = 1f;
        float radin = 0f;
        //蚂蚁线厚度
        float lineThick = 1;

        public MagicWandPage()
        {
            this.InitializeComponent();
            Init();
        }

        async void Init()
        {
            {
                dottedLineCode = await ShaderType.DottedLine.LoadAsync();

                timer.Interval = TimeSpan.FromMilliseconds(100);

                timer.Tick += (s, e) =>
                {
                    if (effectImage == null)
                        return;
                    timeCount++;
                    se = new ScaleEffect()
                    {
                        Source = effectImage,
                        Scale = new Vector2(scale),
                    };

                    using (var ds = crt.CreateDrawingSession())
                    {
                        ds.DrawImage(se);
                    }

                    pse = new PixelShaderEffect(dottedLineCode)
                    {
                        Source1 = crt,
                        Source1Interpolation = CanvasImageInterpolation.NearestNeighbor,
                        Properties = {
                            ["time"] =timeCount*1.5f,
                            ["lineWidth"]=10f,
                            ["color1"]= new Vector3(0),
                            ["color2"]=new Vector3(1),
                            //
                            ["lineGap"]=100f,
                            ["lineSpeed"] =100f,
                        }
                    };

                    var bounds = se.GetBounds(effectCanvas);

                    effectCanvas.Invalidate();
                };

                timer.Start();

                this.Unloaded += (s, e) =>
                {
                    timer.Stop();
                    timer = null;
                };
            }



            selectPicture.Click += async (s, e) =>
            {
                StorageFile file = await PickSingleImageFileAsync(PickerLocationId.Desktop);
                if (file != null)
                {
                    originalImage = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), await file.OpenReadAsync());
                    originalColors = originalImage.GetPixelColors();
                    effectColors = new Color[originalColors.Length];
                    effectCanvas.Width = originalCanvas.Width = originalImage.Size.Width;
                    effectCanvas.Height = originalCanvas.Height = originalImage.Size.Height;
                    effectImage = CanvasBitmap.CreateFromColors(effectCanvas, effectColors, (int)originalImage.Size.Width, (int)originalImage.Size.Height, 96f);
                    crt = new CanvasRenderTarget(effectCanvas, new Size(effectImage.Size.Width * scale, effectImage.Size.Height * scale));

                    imageWidth = (int)originalImage.Size.Width;
                    imageHeight = (int)originalImage.Size.Height;
                    originalCanvas.Invalidate();
                    effectCanvas.Invalidate();
                }
            };

            originalCanvas.Draw += (s, e) =>
            {
                if (originalImage == null)
                    return;
                e.DrawingSession.DrawImage(originalImage);
            };

            effectCanvas.Draw += (s, e) =>
                {
                    if (effectImage == null || dottedLineCode == null)
                        return;
                    var m32 = new Matrix3x2();
                    m32 = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateRotation(radin, new Vector2(0.5f, 0.5f)) * Matrix3x2.CreateTranslation(offset);

                    var crt = new CanvasRenderTarget(effectCanvas, new Size(20, 20));
                    using(var ds = crt.CreateDrawingSession())
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                Color c = (x + y) % 2 == 0 ? Color.FromArgb(255, 128, 128, 128) : Color.FromArgb(255, 200, 200, 200);
                                var dx = x * 10;
                                var dy = y * 10;
                                ds.FillRectangle(new Rect(dx, dy, 10, 10), c);
                            }
                        }
                    }

                    BorderEffect be = new BorderEffect()
                    {
                        Source = crt,
                         ExtendX = CanvasEdgeBehavior.Wrap,
                          ExtendY = CanvasEdgeBehavior.Wrap,
                    };


                    Transform2DEffect te = new Transform2DEffect()
                    {
                        Source = effectImage,
                        TransformMatrix = m32,
                    };
                    using (var ds = effectTarget.CreateDrawingSession())
                    {
                        ds.Clear(Colors.Transparent);
                        ds.DrawImage(te);
                    }

                    pse = new PixelShaderEffect(dottedLineCode)
                    {
                        Source1 = effectTarget,
                        Source1Interpolation = CanvasImageInterpolation.NearestNeighbor,
                        Properties = {
                            ["time"] =(float)e.Timing.TotalTime.TotalSeconds,
                            ["lineWidth"]= lineThick ,
                            ["color1"]= new Vector3(0),
                            ["color2"]=new Vector3(1),
                            ["lineGap"]=100f,
                            ["lineSpeed"] =1f,
                            }
                    };
                    e.DrawingSession.DrawImage(be);
                    e.DrawingSession.DrawImage(pse);
                };

            PointerEventHandler func = (s, e) =>
             {
                 var can = (FrameworkElement)s;
                 var point = e.GetCurrentPoint(can).Position;
                 Vector3 v3 = new Vector3((float)point.X - 15f + can.ActualOffset.X, (float)point.Y - 15f + can.ActualOffset.Y, 0);
                 e1.Translation = e2.Translation = v3;
             };

            originalCanvas.PointerMoved += func;
            effectCanvas.PointerMoved += func;

            TappedEventHandler teh = (s, e) =>
            {
                if (originalImage == null)
                    return;
                var can = (FrameworkElement)s;
                var point = e.GetPosition(can);

                var m32 = new Matrix3x2();
                m32 = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateRotation(radin, new Vector2(0.5f, 0.5f)) * Matrix3x2.CreateTranslation(offset);
                point = Vector2.Transform(point.ToVector2(), m32).ToPoint();
                if (point.X < 0 || point.Y < 0 || point.X > imageWidth || point.Y > imageHeight)
                    return;
                MagicWand(point);
                effectImage = CanvasBitmap.CreateFromColors(effectCanvas, effectColors, (int)originalImage.Size.Width, (int)originalImage.Size.Height, 96f);
                effectCanvas.Invalidate();
            };
            originalCanvas.Tapped += CanvasTapped;
            effectCanvas.Tapped += CanvasTapped;

            //滚轮事件
            PointerEventHandler WheelChanged = (s, e) =>
            {
                FrameworkElement element = (FrameworkElement)s;
                var dir = e.GetCurrentPoint(element).Properties.MouseWheelDelta;
                //缩放
                if (e.KeyModifiers == Windows.System.VirtualKeyModifiers.Control)
                {
                    scale += dir < 0 ? 0.1f : -0.1f;
                }
                //旋转
                else
                {
                    var angle = (float)(2 * Math.PI / 360);
                    radin += dir < 0 ? angle : -angle;
                }
                originalCanvas.Invalidate();
            };
            originalCanvas.PointerWheelChanged += WheelChanged;
            effectCanvas.PointerWheelChanged += WheelChanged;
            originalCanvas.ManipulationMode = effectCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            ManipulationDeltaEventHandler Detal = (s, e) =>
            {
                offset += e.Delta.Translation.ToVector2();
                originalCanvas.Invalidate();
            };

            originalCanvas.ManipulationDelta += Detal;
            effectCanvas.ManipulationDelta += Detal;

            dottedLineThick.ValueChanged += (s, e) =>
            {
                lineThick = (float)dottedLineThick.Value;
            };
        }




        void MagicWand(Point point)
        {
            float threshold = (float)thresholdSlider.Value / 100f;
            Pos cp = new Pos((int)Math.Round(point.X), (int)Math.Round(point.Y));
            Color selectColor = originalColors[cp.X + cp.Y * imageWidth];

            using (var ds = colorMatchTarget.CreateDrawingSession())
            {
                var colorMatchEffect = new PixelShaderEffect(colorMatchCode)
                {
                    Source1 = originalImage,
                    Properties = {
                    ["threshold"] = threshold,
                    ["matchColor"] = new Vector4((float)selectColor.R/255,(float)selectColor.G/255,(float)selectColor.B/255,(float)selectColor.A/255)
                }
                };
                ds.Clear(Colors.Transparent);
                ds.DrawImage(colorMatchEffect);
            }
            var tempColor = colorMatchTarget.GetPixelColors();
            tempDir = new Dictionary<Pos, int>();
            tempDir.Add(cp, 1);
            List<Pos> retrieves = new List<Pos>();
            retrieves.Add(cp);
            while (retrieves.Count > 0)
            {
                var p = retrieves[0];
                retrieves.RemoveAt(0);
                var index = p.X + p.Y * imageWidth;
                Color currentColor = tempColor[index];
                if (currentColor.A > 0)
                {
                    effectColors[index] = originalColors[index];
                    AddRetrieves(p, retrieves);
                }
            }
        }

        void AddRetrieves(Pos cp, List<Pos> retrieves)
        {
            for (int rx = -1; rx <= 1; rx++)
            {
                for (int ry = -1; ry <= 1; ry++)
                {
                    int x = cp.X + rx;
                    int y = cp.Y + ry;
                    if (x >= 0 && y >= 0 && x < imageWidth && y < imageHeight)
                    {
                        Pos pos = new Pos(x, y);
                        if (!tempDir.ContainsKey(pos))
                        {
                            //添加到以检索队列中
                            tempDir.Add(pos, 1);
                            retrieves.Add(pos);
                        }
                        else
                        {
                            var a = "a";
                        }

                    }
                }
            }
        }

        public async static Task<StorageFile> PickSingleImageFileAsync(PickerLocationId location)
        {
            // Picker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = location,
                FileTypeFilter =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".bmp"
                }
            };

            // File
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file;
        }

    }

    struct Pos
    {
        public int X;
        public int Y;
        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
