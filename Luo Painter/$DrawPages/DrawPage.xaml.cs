﻿using Luo_Painter.Blends;
using Luo_Painter.Brushes;
using Luo_Painter.Edits;
using Luo_Painter.Elements;
using Luo_Painter.Historys;
using Luo_Painter.Historys.Models;
using Luo_Painter.Layers;
using Luo_Painter.Layers.Models;
using Luo_Painter.Options;
using Luo_Painter.Shaders;
using Luo_Painter.Tools;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Luo_Painter
{
    [ContentProperty(Name = nameof(Content))]
    internal class OptionCase : DependencyObject, ICase<OptionType>
    {
        public object Content
        {
            get => (object)base.GetValue(ContentProperty);
            set => base.SetValue(ContentProperty, value);
        }
        /// <summary> Identifies the <see cref="Content"/> property. </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(OptionCase), new PropertyMetadata(null));

        public OptionType Value
        {
            get => (OptionType)base.GetValue(ValueProperty);
            set => base.SetValue(ValueProperty, value);
        }
        /// <summary> Identifies the <see cref="Value"/> property. </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(OptionType), typeof(OptionCase), new PropertyMetadata(default(OptionType)));

        public void OnNavigatedTo() { }

        public void OnNavigatedFrom() { }
    }

    [ContentProperty(Name = nameof(SwitchCases))]
    internal sealed class OptionSwitchPresenter : SwitchPresenter<OptionType> { }


    [ContentProperty(Name = nameof(Content))]
    internal class FootCase : DependencyObject, ICase<FootType>
    {
        public object Content
        {
            get => (object)base.GetValue(ContentProperty);
            set => base.SetValue(ContentProperty, value);
        }
        /// <summary> Identifies the <see cref="Content"/> property. </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(FootCase), new PropertyMetadata(null));

        public FootType Value
        {
            get => (FootType)base.GetValue(ValueProperty);
            set => base.SetValue(ValueProperty, value);
        }
        /// <summary> Identifies the <see cref="Value"/> property. </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(FootType), typeof(FootCase), new PropertyMetadata(default(FootType)));

        public void OnNavigatedTo() { }

        public void OnNavigatedFrom() { }
    }

    [ContentProperty(Name = nameof(SwitchCases))]
    internal class FootSwitchPresenter : SwitchPresenter<FootType> { }


    [ContentProperty(Name = nameof(Content))]
    internal class ToolGroupCase : DependencyObject, IGroupCase<ToolGroupType, ToolType>
    {
        public object Content
        {
            get => (object)base.GetValue(ContentProperty);
            set => base.SetValue(ContentProperty, value);
        }
        /// <summary> Identifies the <see cref="Content"/> property. </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(ToolGroupCase), new PropertyMetadata(null));

        public ToolGroupType GroupValue
        {
            get => (ToolGroupType)base.GetValue(GroupValueProperty);
            set => base.SetValue(GroupValueProperty, value);
        }
        /// <summary> Identifies the <see cref="GroupValue"/> property. </summary>
        public static readonly DependencyProperty GroupValueProperty = DependencyProperty.Register(nameof(ToolGroupType), typeof(ToolGroupType), typeof(ToolGroupCase), new PropertyMetadata(default(ToolGroupType)));

        public ToolType Value
        {
            get => (ToolType)base.GetValue(ValueProperty);
            set => base.SetValue(ValueProperty, value);
        }
        /// <summary> Identifies the <see cref="Value"/> property. </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(ToolType), typeof(ToolGroupCase), new PropertyMetadata(default(ToolType)));

        public void OnNavigatedTo() { }

        public void OnNavigatedFrom() { }
    }

    [ContentProperty(Name = nameof(SwitchCases))]
    internal class ToolGroupSwitchPresenter : SwitchGroupPresenter<ToolGroupType, ToolType> { }


    public sealed partial class DrawPage : Page
    {

        //@Key
        private bool IsKeyDown(VirtualKey key) => Window.Current.CoreWindow.GetKeyState(key).HasFlag(CoreVirtualKeyStates.Down);
        private bool IsCtrl => this.IsKeyDown(VirtualKey.Control);
        private bool IsShift => this.IsKeyDown(VirtualKey.Shift);
        private bool IsAlt => this.IsKeyDown(VirtualKey.Menu);
        private bool IsSpace => this.IsKeyDown(VirtualKey.Space);

        //@Converter
        private bool ReverseBooleanConverter(bool value) => !value;
        private bool ReverseBooleanConverter(bool? value) => value == false;
        private Visibility BooleanToVisibilityConverter(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        //@Converter
        private Vector2 ToPosition(Vector2 point) => Vector2.Transform(this.CanvasVirtualControl.Dpi.ConvertDipsToPixels(point), this.Transformer.GetInverseMatrix());
        private Vector2 ToPoint(Vector2 position) => this.CanvasVirtualControl.Dpi.ConvertPixelsToDips(Vector2.Transform(position, this.Transformer.GetMatrix()));

        CanvasDevice CanvasDevice { get; } = new CanvasDevice();
        Historian<IHistory> History { get; } = new Historian<IHistory>();
        IDictionary<string, ILayer> Layers { get; } = new Dictionary<string, ILayer>();
        ObservableCollection<ILayer> ObservableCollection { get; } = new ObservableCollection<ILayer>();
        InkPresenter InkPresenter { get; } = new InkPresenter();

        BitmapLayer BitmapLayer { get; set; }
        BitmapLayer Clipboard { get; set; }
        BitmapLayer Marquee { get; set; }
        BitmapLayer Displacement { get; set; }
        bool IsFullScreen { get; set; }

        SelectionType SelectionType { get; set; } = SelectionType.None;
        FootType FootType { get; set; } = FootType.None;
        OptionType OptionType { get; set; } = OptionType.PaintBrush;
        EditType EditType { get; set; } = EditType.None;
        ToolType ToolType { get; set; } = ToolType.PaintBrush;
        InkType InkType { get; set; } = InkType.None;

        byte[] LiquefactionShaderCodeBytes;
        byte[] FreeTransformShaderCodeBytes;
        byte[] GradientMappingShaderCodeBytes;
        byte[] RippleEffectShaderCodeBytes;
        byte[] DifferenceShaderCodeBytes;
        byte[] DottedLineTransformShaderCodeBytes;
        byte[] LalphaMaskShaderCodeBytes;
        byte[] RalphaMaskShaderCodeBytes;
        byte[] DisplacementLiquefactionShaderCodeBytes;
        byte[] BrushEdgeHardnessShaderCodeBytes;
        byte[] BrushEdgeHardnessWithTextureShaderCodeBytes;

        private async Task CreateResourcesAsync()
        {
            this.LiquefactionShaderCodeBytes = await ShaderType.Liquefaction.LoadAsync();
            this.FreeTransformShaderCodeBytes = await ShaderType.FreeTransform.LoadAsync();
            this.GradientMappingShaderCodeBytes = await ShaderType.GradientMapping.LoadAsync();
            this.RippleEffectShaderCodeBytes = await ShaderType.RippleEffect.LoadAsync();
            this.DifferenceShaderCodeBytes = await ShaderType.Difference.LoadAsync();
            this.LalphaMaskShaderCodeBytes = await ShaderType.LalphaMask.LoadAsync();
            this.RalphaMaskShaderCodeBytes = await ShaderType.RalphaMask.LoadAsync();
            this.DisplacementLiquefactionShaderCodeBytes = await ShaderType.DisplacementLiquefaction.LoadAsync();
            this.BrushEdgeHardnessShaderCodeBytes = await ShaderType.BrushEdgeHardness.LoadAsync();
            this.BrushEdgeHardnessWithTextureShaderCodeBytes = await ShaderType.BrushEdgeHardnessWithTexture.LoadAsync();
        }
        private async Task CreateDottedLineResourcesAsync()
        {
            this.DottedLineTransformShaderCodeBytes = await ShaderType.DottedLineTransform.LoadAsync();
        }

        public DrawPage()
        {
            this.InitializeComponent();
            this.ConstructCanvas();
            this.ConstructOperator();
            this.ConstructSimulater();

            this.ConstructLayers();
            this.ConstructLayer();

            this.ConstructFoots();
            this.ConstructFoot();

            this.ConstructEdits();

            this.ConstructOptions();
            this.ConstructGradientMapping();
            this.ConstructRippleEffect();

            this.ConstructTools();
            this.ConstructMarquee();
            this.ConstructPaint();
            this.ConstructVector();

            this.ConstructHistory();

            this.ConstructDialog();
            this.ConstructColor();
            this.ConstructStoryboard();


            this.ApplicationView.Title = "*Untitled";

            this.LightDismissOverlay.Tapped += (s, e) => this.ExpanderLightDismissOverlay.Hide();
            this.ExpanderLightDismissOverlay.IsFlyoutChanged += (s, isFlyout) => this.LightDismissOverlay.Visibility = isFlyout ? Visibility.Visible : Visibility.Collapsed;

            this.ExportButton.Click += (s, e) => this.ExportMenu.Toggle(this.ExportButton, ExpanderPlacementMode.Bottom);
            this.ToolButton.Click += (s, e) => this.ToolMenu.Toggle(this.ToolButton, ExpanderPlacementMode.Bottom);
            this.PaintButton.Click += (s, e) => this.PaintMenu.Toggle(this.PaintButton, ExpanderPlacementMode.Bottom);
            this.EditButton.Click += (s, e) => this.EditMenu.Toggle(this.EditButton, ExpanderPlacementMode.Bottom);
            this.OptionButton.Click += (s, e) => this.OptionMenu.Toggle(this.OptionButton, ExpanderPlacementMode.Bottom);
            this.MoreOptionButton.Click += (s, e) => this.MoreOptionMenu.Toggle(this.MoreOptionButton, ExpanderPlacementMode.Bottom);
            this.SetupButton.Click += (s, e) => this.SetupMenu.Toggle(this.SetupButton, ExpanderPlacementMode.Bottom);
            this.LayerButton.Click += (s, e) => this.LayerMenu.Toggle(this.LayerButton, ExpanderPlacementMode.Bottom);
            this.ColorButton.Click += (s, e) => this.ColorMenu.Toggle(this.ColorButton, ExpanderPlacementMode.Bottom);


            // Drag and Drop 
            base.AllowDrop = true;
            base.Drop += async (s, e) =>
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    this.AddAsync(from file in await e.DataView.GetStorageItemsAsync() where file is IStorageFile select file as IStorageFile);
                }
            };
            base.DragOver += (s, e) =>
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                //e.DragUIOverride.Caption = 
                e.DragUIOverride.IsCaptionVisible = e.DragUIOverride.IsContentVisible = e.DragUIOverride.IsGlyphVisible = true;
            };
        }
    }
}