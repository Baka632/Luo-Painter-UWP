﻿using FanKit.Transformers;
using Luo_Painter.Elements;
using Luo_Painter.Historys;
using Luo_Painter.Historys.Models;
using Luo_Painter.Layers;
using Luo_Painter.Layers.Models;
using Luo_Painter.Options;
using Microsoft.Graphics.Canvas;
using System;
using System.Linq;
using System.Numerics;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter
{
    public sealed partial class DrawPage : Page, ILayerManager
    {

        public async void Click(OptionType type)
        {
            switch (type)
            {
                //case OptionType.None:
                //    break;
                //case OptionType.IsItemClickEnabled:
                //    break;
                //case OptionType.ExistIcon:
                //    break;
                //case OptionType.ExistThumbnail:
                //    break;
                //case OptionType.AllowDrag:
                //    break;
                //case OptionType.HasPreview:
                //    break;
                //case OptionType.HasDifference:
                //    break;
                //case OptionType.TempOverlay:
                //    break;
                //case OptionType.HasState:
                //    break;
                //case OptionType.Edit:
                //    break;
                //case OptionType.Option:
                //    break;
                //case OptionType.Tool:
                //    break;
                //case OptionType.Layering:
                //    break;
                //case OptionType.Editing:
                //    break;
                //case OptionType.Grouping:
                //    break;
                //case OptionType.Select:
                //    break;
                //case OptionType.Combine:
                //    break;
                //case OptionType.Setup:
                //    break;
                case OptionType.Remove:
                    {
                        var items = this.LayerSelectedItems;
                        switch (items.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                if (this.LayerSelectedItem is ILayer layer)
                                {
                                    /// History
                                    int removes = this.History.Push(this.Remove(layer));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                            default:
                                {
                                    /// History
                                    int removes = this.History.Push(this.Remove(items));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                        }
                    }
                    break;
                case OptionType.AddLayer:
                    {
                        ILayer add = new BitmapLayer(this.CanvasDevice, this.Transformer.Width, this.Transformer.Height);

                        /// History
                        int removes = this.History.Push(this.Add(add));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.AddImageLayer:
                    this.AddAsync();
                    break;
                case OptionType.CutLayer:
                    {
                        var items = this.LayerSelectedItems;
                        switch (items.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                if (this.LayerSelectedItem is ILayer layer)
                                {
                                    /// History
                                    int removes = this.History.Push(this.Cut(layer));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                    this.LayerMenu.PasteIsEnabled = this.ClipboardLayers.Count is 0 is false;
                                }
                                break;
                            default:
                                {
                                    /// History
                                    int removes = this.History.Push(this.Cut(items));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                    this.LayerMenu.PasteIsEnabled = this.ClipboardLayers.Count is 0 is false;
                                }
                                break;
                        }
                    }
                    break;
                case OptionType.CopyLayer:
                    {
                        /// History
                        var items = this.LayerSelectedItems;
                        switch (items.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                if (this.LayerSelectedItem is ILayer layer)
                                    this.Copy(layer);
                                this.LayerMenu.PasteIsEnabled = this.ClipboardLayers.Count is 0 is false;
                                break;
                            default:
                                this.Copy(items);
                                this.LayerMenu.PasteIsEnabled = this.ClipboardLayers.Count is 0 is false;
                                break;
                        }
                    }
                    break;
                case OptionType.PasteLayer:
                    {
                        switch (this.ClipboardLayers.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                string id = this.ClipboardLayers.Single();
                                if (LayerDictionary.Instance.ContainsKey(id))
                                {
                                    /// History
                                    int removes = this.History.Push(this.Paste(this.CanvasDevice, this.Transformer.Width, this.Transformer.Height, id));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                            default:
                                {
                                    /// History
                                    int removes = this.History.Push(this.Paste(this.CanvasDevice, this.Transformer.Width, this.Transformer.Height, this.ClipboardLayers));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                        }
                    }
                    break;
                case OptionType.Cut: // Copy + Clear
                    {
                        if (this.LayerSelectedItem is BitmapLayer bitmapLayer)
                        {
                            Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                            PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                            switch (mode)
                            {
                                case PixelBoundsMode.None:
                                    this.Clipboard.DrawCopy(bitmapLayer.GetMask(this.Marquee));

                                    // History
                                    int removes = this.History.Push(bitmapLayer.Clear(this.Marquee, interpolationColors));
                                    bitmapLayer.Flush();
                                    bitmapLayer.RenderThumbnail();
                                    break;
                                default:
                                    this.Clipboard.CopyPixels(bitmapLayer);

                                    // History
                                    int removes2 = this.History.Push(bitmapLayer.GetBitmapClearHistory(Colors.Transparent));
                                    bitmapLayer.Clear(Colors.Transparent, BitmapType.Origin);
                                    bitmapLayer.Clear(Colors.Transparent, BitmapType.Source);
                                    bitmapLayer.ClearThumbnail(Colors.Transparent);
                                    break;
                            }

                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                            this.UndoButton.IsEnabled = this.History.CanUndo;
                            this.RedoButton.IsEnabled = this.History.CanRedo;
                            this.LayerMenu.PasteIsEnabled = true;
                        }
                        else
                        {
                            this.Tip("No Layer", "Create a new Layer?");
                        }
                    }
                    break;
                case OptionType.Duplicate: // CopyLayer + PasteLayer
                    this.Click(OptionType.CopyLayer);
                    this.Click(OptionType.PasteLayer);
                    break;
                case OptionType.Copy:
                    {
                        if (this.LayerSelectedItem is BitmapLayer bitmapLayer)
                        {
                            Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                            PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                            switch (mode)
                            {
                                case PixelBoundsMode.None:
                                    this.Clipboard.DrawCopy(bitmapLayer.GetMask(this.Marquee));
                                    break;
                                default:
                                    this.Clipboard.CopyPixels(bitmapLayer);
                                    break;
                            }

                            this.LayerMenu.PasteIsEnabled = true;
                        }
                    }
                    break;
                case OptionType.Paste:
                    {
                        Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                        PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                        if (mode is PixelBoundsMode.Transarent)
                        {
                            this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                            break;
                        }

                        BitmapLayer add = new BitmapLayer(this.CanvasDevice, this.Clipboard[BitmapType.Source], this.Transformer.Width, this.Transformer.Height);

                        // History
                        int removes = this.History.Push(this.Add(add));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.Clear:
                    {
                        if (this.LayerSelectedItem is BitmapLayer bitmapLayer)
                        {
                            Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                            PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                            switch (mode)
                            {
                                case PixelBoundsMode.None:
                                    // History
                                    int removes = this.History.Push(bitmapLayer.Clear(this.Marquee, interpolationColors));
                                    bitmapLayer.Flush();
                                    bitmapLayer.RenderThumbnail();
                                    break;
                                default:
                                    // History
                                    int removes2 = this.History.Push(bitmapLayer.GetBitmapClearHistory(Colors.Transparent));
                                    bitmapLayer.Clear(Colors.Transparent, BitmapType.Origin);
                                    bitmapLayer.Clear(Colors.Transparent, BitmapType.Source);
                                    bitmapLayer.ClearThumbnail(Colors.Transparent);
                                    break;
                            }

                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                            this.UndoButton.IsEnabled = this.History.CanUndo;
                            this.RedoButton.IsEnabled = this.History.CanRedo;
                        }
                    }
                    break;
                case OptionType.Extract:
                    {
                        if (this.LayerSelectedItem is BitmapLayer bitmapLayer)
                        {
                            Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                            PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                            if (mode is PixelBoundsMode.Transarent)
                            {
                                this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                break;
                            }

                            BitmapLayer add;
                            switch (mode)
                            {
                                case PixelBoundsMode.None:
                                    add = new BitmapLayer(this.CanvasDevice, bitmapLayer.GetMask(this.Marquee), this.Transformer.Width, this.Transformer.Height);
                                    break;
                                default:
                                    add = new BitmapLayer(this.CanvasDevice, bitmapLayer);
                                    break;
                            }

                            // History
                            int removes = this.History.Push(this.Add(add));

                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                            this.UndoButton.IsEnabled = this.History.CanUndo;
                            this.RedoButton.IsEnabled = this.History.CanRedo;
                        }
                    }
                    break;
                case OptionType.Merge:
                    {
                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            switch (layer.Type)
                            {
                                case LayerType.Bitmap:
                                    if (layer is BitmapLayer bitmapLayer)
                                    {
                                        if (this.ObservableCollection.GetNeighbor(layer) is ILayer neighbor)
                                        {
                                            if (neighbor.Merge(bitmapLayer, bitmapLayer[BitmapType.Origin]) is ICanvasImage source)
                                            {
                                                /// History
                                                bitmapLayer.DrawCopy(source);
                                                int removes1 = this.History.Push(bitmapLayer.GetBitmapResetHistory());
                                                bitmapLayer.Flush();
                                                bitmapLayer.RenderThumbnail();

                                                /// History
                                                int removes2 = this.History.Push(this.Remove(neighbor));

                                                this.CanvasVirtualControl.Invalidate(); // Invalidate

                                                this.UndoButton.IsEnabled = this.History.CanUndo;
                                                this.RedoButton.IsEnabled = this.History.CanRedo;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case OptionType.Flatten:
                    {
                        ICanvasImage image = this.Nodes.Merge(null, null);
                        ILayer add = new BitmapLayer(this.CanvasDevice, image, this.Transformer.Width, this.Transformer.Height);

                        /// History
                        int removes = this.History.Push(this.Clear(add));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.Group:
                    {
                        var items = this.LayerSelectedItems;
                        switch (items.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                if (this.LayerSelectedItem is ILayer layer)
                                {
                                    ILayer add = new GroupLayer(this.CanvasDevice, this.Transformer.Width, this.Transformer.Height);

                                    /// History
                                    int removes = this.History.Push(this.Group(add, layer));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                            default:
                                {
                                    ILayer add = new GroupLayer(this.CanvasDevice, this.Transformer.Width, this.Transformer.Height);

                                    /// History
                                    int removes = this.History.Push(this.Group(add, items));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                        }
                    }
                    break;
                case OptionType.Ungroup:
                    {
                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Children.Count is 0) break;

                            /// History
                            int removes = this.History.Push(this.Ungroup(layer));

                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                            this.UndoButton.IsEnabled = this.History.CanUndo;
                            this.RedoButton.IsEnabled = this.History.CanRedo;
                        }
                    }
                    break;
                case OptionType.Release:
                    {
                        var items = this.LayerSelectedItems;
                        switch (items.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                if (this.LayerSelectedItem is ILayer layer)
                                {
                                    if (this.Release(layer) is IHistory history)
                                    {
                                        /// History
                                        int removes = this.History.Push(history);

                                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                                        this.UndoButton.IsEnabled = this.History.CanUndo;
                                        this.RedoButton.IsEnabled = this.History.CanRedo;
                                    }
                                }
                                break;
                            default:
                                {
                                    /// History
                                    int removes = this.History.Push(this.Release(items));

                                    this.CanvasVirtualControl.Invalidate(); // Invalidate

                                    this.UndoButton.IsEnabled = this.History.CanUndo;
                                    this.RedoButton.IsEnabled = this.History.CanRedo;
                                }
                                break;
                        }
                    }
                    break;
                case OptionType.All:
                    {
                        // History
                        int removes = this.History.Push(this.Marquee.GetBitmapClearHistory(Colors.DodgerBlue));
                        this.Marquee.Clear(Colors.DodgerBlue, BitmapType.Origin);
                        this.Marquee.Clear(Colors.DodgerBlue, BitmapType.Source);
                        this.Marquee.ClearThumbnail(Colors.DodgerBlue);

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.Deselect:
                    {
                        // History
                        int removes = this.History.Push(this.Marquee.GetBitmapClearHistory(Colors.Transparent));
                        this.Marquee.Clear(Colors.Transparent, BitmapType.Origin);
                        this.Marquee.Clear(Colors.Transparent, BitmapType.Source);
                        this.Marquee.ClearThumbnail(Colors.Transparent);

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.MarqueeInvert:
                    {
                        // History
                        int removes = this.History.Push(this.Marquee.Invert(Colors.DodgerBlue));
                        this.Marquee.Flush();
                        this.Marquee.RenderThumbnail();

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.Pixel:
                    {
                        if (this.LayerSelectedItem is BitmapLayer bitmapLayer)
                        {
                            // History
                            int removes = this.History.Push(this.Marquee.Pixel(bitmapLayer, Colors.DodgerBlue));
                            this.Marquee.Flush();
                            this.Marquee.RenderThumbnail();

                            this.UndoButton.IsEnabled = this.History.CanUndo;
                            this.RedoButton.IsEnabled = this.History.CanRedo;
                        }
                    }
                    break;
                case OptionType.Feather:
                    {
                        Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                        PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                        if (mode is PixelBoundsMode.Transarent)
                        {
                            this.Tip("No Pixel", "The Marquee is Transparent.");
                            break;
                        }

                        this.OptionType = type;
                        this.SetFootType(type);
                        this.SetCanvasState(true);
                        break;
                    }
                    break;
                case OptionType.MarqueeTransform:
                    {
                        Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                        PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                        if (mode is PixelBoundsMode.Transarent)
                        {
                            this.Tip("No Pixel", "The Marquee is Transparent.");
                            break;
                        }

                        switch (mode)
                        {
                            case PixelBoundsMode.None:
                                PixelBounds interpolationBounds = this.Marquee.CreateInterpolationBounds(interpolationColors);
                                PixelBounds bounds = this.Marquee.CreatePixelBounds(interpolationBounds, interpolationColors);
                                this.SetTransform(bounds);
                                break;
                            default:
                                this.SetTransform(this.Marquee.Bounds);
                                break;
                        }

                        this.OptionType = type;
                        this.SetFootType(type);
                        this.SetCanvasState(true);
                        break;
                    }
                    break;
                case OptionType.Grow:
                    {
                        Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                        PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                        if (mode is PixelBoundsMode.Transarent)
                        {
                            this.Tip("No Pixel", "The Marquee is Transparent.");
                            break;
                        }

                        this.OptionType = type;
                        this.SetFootType(type);
                        this.SetCanvasState(true);
                        break;
                    }
                    break;
                case OptionType.Shrink:
                    {
                        Color[] interpolationColors = this.Marquee.GetInterpolationColorsBySource();
                        PixelBoundsMode mode = this.Marquee.GetInterpolationBoundsMode(interpolationColors);

                        if (mode is PixelBoundsMode.Transarent)
                        {
                            this.Tip("No Pixel", "The Marquee is Transparent.");
                            break;
                        }

                        this.OptionType = type;
                        this.SetFootType(type);
                        this.SetCanvasState(true);
                        break;
                    }
                    break;
                case OptionType.Union:
                    break;
                case OptionType.Exclude:
                    break;
                case OptionType.Xor:
                    break;
                case OptionType.Intersect:
                    break;
                case OptionType.ExpandStroke:
                    break;
                case OptionType.CropCanvas:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        int width2 = this.Transformer.Width;
                        int height2 = this.Transformer.Height;

                        this.SetCropCanvas(width2, height2);

                        this.OptionType = OptionType.CropCanvas;
                        this.SetFootType(OptionType.CropCanvas);
                        this.SetCanvasState(true);
                    }
                    break;
                case OptionType.Stretch:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        ContentDialogResult result = await this.StretchDialog.ShowInstance();
                        switch (result)
                        {
                            case ContentDialogResult.Primary:
                                int width2 = this.Transformer.Width;
                                int height2 = this.Transformer.Height;

                                uint width = (uint)width2;
                                uint height = (uint)height2;

                                uint w2 = this.StretchDialog.Size.Width;
                                uint h2 = this.StretchDialog.Size.Height;
                                if (w2 == width && h2 == height) break;

                                int w = (int)w2;
                                int h = (int)h2;

                                switch (this.StretchDialog.SelectedIndex)
                                {
                                    case 0:
                                        {
                                            CanvasImageInterpolation interpolation = this.StretchDialog.Interpolation;
                                            {
                                                this.Transformer.Width = w;
                                                this.Transformer.Height = h;
                                                this.Transformer.Fit();

                                                this.CreateResources(w, h);
                                                this.CreateMarqueeResources(w, h);
                                            }
                                            int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.Skretch(this.CanvasDevice, w, h, interpolation)).ToArray(), new SetupSizes
                                            {
                                                UndoParameter = new BitmapSize { Width = width, Height = height },
                                                RedoParameter = new BitmapSize { Width = w2, Height = h2 }
                                            }));

                                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                                            this.UndoButton.IsEnabled = this.History.CanUndo;
                                            this.RedoButton.IsEnabled = this.History.CanRedo;
                                        }
                                        break;
                                    case 1:
                                        {
                                            IndicatorMode indicator = this.StretchDialog.Indicator;

                                            Vector2 vect = this.Transformer.GetIndicatorVector(indicator);
                                            {
                                                this.Transformer.Width = w;
                                                this.Transformer.Height = h;
                                                this.Transformer.Fit();

                                                this.CreateResources(w, h);
                                                this.CreateMarqueeResources(w, h);
                                            }
                                            Vector2 vect2 = this.Transformer.GetIndicatorVector(indicator);

                                            Vector2 offset = vect2 - vect;
                                            int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.Crop(this.CanvasDevice, w, h, offset)).ToArray(), new SetupSizes
                                            {
                                                UndoParameter = new BitmapSize { Width = width, Height = height },
                                                RedoParameter = new BitmapSize { Width = w2, Height = h2 }
                                            }));

                                            this.CanvasVirtualControl.Invalidate(); // Invalidate

                                            this.UndoButton.IsEnabled = this.History.CanUndo;
                                            this.RedoButton.IsEnabled = this.History.CanRedo;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case OptionType.FlipHorizontal:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        this.Transformer.Fit();

                        int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.FlipHorizontal(this.CanvasDevice)).ToArray(), null));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.FlipVertical:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        this.Transformer.Fit();

                        int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.FlipVertical(this.CanvasDevice)).ToArray(), null));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.LeftTurn:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        int width2 = this.Transformer.Width;
                        int height2 = this.Transformer.Height;

                        uint width = (uint)width2;
                        uint height = (uint)height2;
                        {
                            int w = height2;
                            int h = width2;

                            this.Transformer.Width = w;
                            this.Transformer.Height = h;
                            this.Transformer.Fit();

                            this.CreateResources(w, h);
                            this.CreateMarqueeResources(w, h);
                        }
                        int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.LeftTurn(this.CanvasDevice)).ToArray(), width2 == height2 ? null : new SetupSizes
                        {
                            UndoParameter = new BitmapSize { Width = width, Height = height },
                            RedoParameter = new BitmapSize { Width = height, Height = width }
                        }));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.RightTurn:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        int width2 = this.Transformer.Width;
                        int height2 = this.Transformer.Height;

                        uint width = (uint)width2;
                        uint height = (uint)height2;
                        {
                            int w = height2;
                            int h = width2;

                            this.Transformer.Width = w;
                            this.Transformer.Height = h;
                            this.Transformer.Fit();

                            this.CreateResources(w, h);
                            this.CreateMarqueeResources(w, h);
                        }
                        int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.RightTurn(this.CanvasDevice)).ToArray(), width2 == height2 ? null : new SetupSizes
                        {
                            UndoParameter = new BitmapSize { Width = width, Height = height },
                            RedoParameter = new BitmapSize { Width = height, Height = width }
                        }));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                case OptionType.OverTurn:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        this.Transformer.Fit();

                        int removes = this.History.Push(this.Setup(this.Nodes.Select(c => c.OverTurn(this.CanvasDevice)).ToArray(), null));

                        this.CanvasVirtualControl.Invalidate(); // Invalidate

                        this.UndoButton.IsEnabled = this.History.CanUndo;
                        this.RedoButton.IsEnabled = this.History.CanRedo;
                    }
                    break;
                //case OptionType.More:
                //    break;
                //case OptionType.Adjustment:
                //    break;
                //case OptionType.Effect1:
                //    break;
                //case OptionType.Effect2:
                //    break;
                //case OptionType.Effect3:
                //    break;
                case OptionType.Transform:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Type is LayerType.Bitmap && layer is BitmapLayer bitmapLayer)
                            {
                                SelectionType state = bitmapLayer.GetSelection(this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
                                if (state is SelectionType.None)
                                {
                                    this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                    break;
                                }

                                switch (state)
                                {
                                    case SelectionType.PixelBounds:
                                        {
                                            PixelBounds interpolationBounds = bitmapLayer.CreateInterpolationBounds(InterpolationColors);
                                            PixelBounds bounds = bitmapLayer.CreatePixelBounds(interpolationBounds);
                                            this.SetTransform(bounds);
                                        }
                                        break;
                                    case SelectionType.MarqueePixelBounds:
                                        {
                                            PixelBounds interpolationBounds = this.Marquee.CreateInterpolationBounds(InterpolationColors);
                                            PixelBounds bounds = this.Marquee.CreatePixelBounds(interpolationBounds);
                                            this.SetTransform(bounds);
                                        }
                                        break;
                                    default:
                                        this.SetTransform(bitmapLayer.Bounds);
                                        break;
                                }

                                this.BitmapLayer = bitmapLayer;
                                this.SelectionType = state;
                                this.OptionType = type;
                                this.SetFootType(type);
                                this.SetCanvasState(true);
                                break;
                            }
                            else this.Tip("Not Bitmap Layer", "Can only operate on Bitmap Layer.");
                        }
                        else this.Tip("No Layer", "Create a new Layer?");
                    }
                    break;
                case OptionType.DisplacementLiquefaction:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Type is LayerType.Bitmap && layer is BitmapLayer bitmapLayer)
                            {
                                SelectionType state = bitmapLayer.GetSelection(this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
                                if (state is SelectionType.None)
                                {
                                    this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                    break;
                                }

                                switch (type)
                                {
                                    case OptionType.Transform:
                                        switch (state)
                                        {
                                            case SelectionType.PixelBounds:
                                                {
                                                    PixelBounds interpolationBounds = bitmapLayer.CreateInterpolationBounds(InterpolationColors);
                                                    PixelBounds bounds = bitmapLayer.CreatePixelBounds(interpolationBounds);
                                                    this.SetTransform(bounds);
                                                }
                                                break;
                                            case SelectionType.MarqueePixelBounds:
                                                {
                                                    PixelBounds interpolationBounds = this.Marquee.CreateInterpolationBounds(InterpolationColors);
                                                    PixelBounds bounds = this.Marquee.CreatePixelBounds(interpolationBounds);
                                                    this.SetTransform(bounds);
                                                }
                                                break;
                                            default:
                                                this.SetTransform(bitmapLayer.Bounds);
                                                break;
                                        }
                                        break;
                                    case OptionType.DisplacementLiquefaction:
                                        this.SetDisplacementLiquefaction();
                                        break;
                                    case OptionType.GradientMapping:
                                        this.SetGradientMapping();
                                        break;
                                }

                                this.BitmapLayer = bitmapLayer;
                                this.SelectionType = state;
                                this.OptionType = type;
                                this.SetFootType(type);
                                this.SetCanvasState(true);
                                break;
                            }
                            else this.Tip("Not Bitmap Layer", "Can only operate on Bitmap Layer.");
                        }
                        else this.Tip("No Layer", "Create a new Layer?");
                    }
                    break;
                case OptionType.GradientMapping:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Type is LayerType.Bitmap && layer is BitmapLayer bitmapLayer)
                            {
                                SelectionType state = bitmapLayer.GetSelection(this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
                                if (state is SelectionType.None)
                                {
                                    this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                    break;
                                }

                                this.SetGradientMapping();

                                this.BitmapLayer = bitmapLayer;
                                this.SelectionType = state;
                                this.OptionType = type;
                                this.SetFootType(type);
                                this.SetCanvasState(true);
                                break;
                            }
                            else this.Tip("Not Bitmap Layer", "Can only operate on Bitmap Layer.");
                        }
                        else this.Tip("No Layer", "Create a new Layer?");
                    }
                    break;
                case OptionType.RippleEffect:
                case OptionType.Fill:
                case OptionType.Exposure:
                case OptionType.Brightness:
                case OptionType.Saturation:
                case OptionType.HueRotation:
                case OptionType.Contrast:
                case OptionType.Temperature:
                case OptionType.HighlightsAndShadows:
                case OptionType.LuminanceToAlpha:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Type is LayerType.Bitmap && layer is BitmapLayer bitmapLayer)
                            {
                                SelectionType state = bitmapLayer.GetSelection(this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
                                if (state is SelectionType.None)
                                {
                                    this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                    break;
                                }

                                this.BitmapLayer = bitmapLayer;
                                this.SelectionType = state;
                                this.OptionType = type;
                                this.SetFootType(type);
                                this.SetCanvasState(true);
                                break;
                            }
                            else this.Tip("Not Bitmap Layer", "Can only operate on Bitmap Layer.");
                        }
                        else this.Tip("No Layer", "Create a new Layer?");
                    }
                    break;
                case OptionType.Gray:
                case OptionType.Invert:
                    {
                        this.ExpanderLightDismissOverlay.Hide();

                        if (this.LayerSelectedItem is ILayer layer)
                        {
                            if (layer.Type is LayerType.Bitmap && layer is BitmapLayer bitmapLayer)
                            {
                                SelectionType state = bitmapLayer.GetSelection(this.Marquee, out Color[] InterpolationColors, out PixelBoundsMode mode);
                                if (state is SelectionType.None)
                                {
                                    this.Tip("No Pixel", "The current Bitmap Layer is Transparent.");
                                    break;
                                }

                                this.Primary(bitmapLayer, this.GetPreview(type, bitmapLayer[BitmapType.Origin]));
                                break;
                            }
                            else this.Tip("Not Bitmap Layer", "Can only operate on Bitmap Layer.");
                        }
                        else this.Tip("No Layer", "Create a new Layer?");
                    }
                    break;
                case OptionType.GaussianBlur:
                case OptionType.DirectionalBlur:
                case OptionType.Sharpen:
                case OptionType.Shadow:
                case OptionType.ChromaKey:
                case OptionType.EdgeDetection:
                case OptionType.Border:
                case OptionType.Emboss:
                case OptionType.Lighting:
                case OptionType.Fog:
                case OptionType.Sepia:
                case OptionType.Posterize:
                case OptionType.Colouring:
                case OptionType.Tint:
                case OptionType.DiscreteTransfer:
                case OptionType.Vignette:
                case OptionType.GammaTransfer:
                case OptionType.Glass:
                case OptionType.PinchPunch:
                case OptionType.Morphology:
                    break;
                //case OptionType.Marquee:
                //    break;
                //case OptionType.Selection:
                //    break;
                //case OptionType.Paint:
                //    break;
                //case OptionType.Vector:
                //    break;
                //case OptionType.Curve:
                //    break;
                //case OptionType.Text:
                //    break;
                //case OptionType.Geometry:
                //    break;
                //case OptionType.Pattern:
                //    break;
                case OptionType.MarqueeRectangular:
                case OptionType.MarqueeElliptical:
                case OptionType.MarqueePolygon:
                case OptionType.MarqueeFreeHand:
                case OptionType.SelectionFlood:
                case OptionType.SelectionBrush:
                case OptionType.PaintBrush:
                case OptionType.PaintWatercolorPen:
                case OptionType.PaintPencil:
                case OptionType.PaintEraseBrush:
                case OptionType.PaintLiquefaction:
                case OptionType.Cursor:
                case OptionType.View:
                case OptionType.Crop:
                case OptionType.Brush:
                case OptionType.Transparency:
                case OptionType.Image:
                case OptionType.Node:
                case OptionType.Pen:
                case OptionType.TextArtistic:
                case OptionType.TextFrame:
                case OptionType.GeometryRectangle:
                case OptionType.GeometryEllipse:
                case OptionType.GeometryRoundRect:
                case OptionType.GeometryTriangle:
                case OptionType.GeometryDiamond:
                case OptionType.GeometryPentagon:
                case OptionType.GeometryStar:
                case OptionType.GeometryCog:
                case OptionType.GeometryDount:
                case OptionType.GeometryPie:
                case OptionType.GeometryCookie:
                case OptionType.GeometryArrow:
                case OptionType.GeometryCapsule:
                case OptionType.GeometryHeart:
                case OptionType.PatternGrid:
                case OptionType.PatternDiagonal:
                case OptionType.PatternSpotted:
                    {
                        this.ToolMenu.Title = type.ToString();

                        this.ToolResource.Source = new Uri(type.GetResource());
                        this.ToolIcon.Template = type.GetTemplate(this.ToolResource);

                        this.ToolSwitchPresenter.Value = type;

                        this.SetInkToolType(type);

                        this.OptionType = type;
                        this.SetFootType(type);
                        this.SetCanvasState(default);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}