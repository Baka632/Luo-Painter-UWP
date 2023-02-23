﻿using FanKit.Transformers;
using Luo_Painter.Blends;
using Luo_Painter.Brushes;
using Luo_Painter.Elements;
using Luo_Painter.Options;
using Luo_Painter.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Luo_Painter
{
    internal sealed class MainPageToDrawPageAttribute : Attribute
    {
        readonly NavigationMode NavigationMode;
        public MainPageToDrawPageAttribute(NavigationMode navigationMode) => this.NavigationMode = navigationMode;
        public override string ToString() => $"{typeof(MainPage)} to {typeof(DrawPage)}, Parameter is {typeof(ProjectParameter)}, NavigationMode is {this.NavigationMode}";
    }
    internal sealed class DrawPageToStylePageAttribute : Attribute
    {
        readonly NavigationMode NavigationMode;
        public DrawPageToStylePageAttribute(NavigationMode navigationMode) => this.NavigationMode = navigationMode;
        public override string ToString() => $"{typeof(DrawPage)} to {typeof(StylePage)}, NavigationMode is {this.NavigationMode}";
    }

    // 1. It is a UserControl.xaml that contains a ListView.
    // <UserControl>
    //      <ListView>
    //      ...
    //      </ListView>
    // </UserControl>
    // Ok.

    // 2. It is a UserControl.xaml, RootNode is ListView.
    // <ListView>
    //      ...
    // </ListView>
    // Exception:
    // Windows.UI.Xaml.Markup.XamlParseException:
    // “XAML parsing failed.”
    // Why ?

    // 3. It is a UserControl.xaml, RootNode is XamlListView.
    // <local:XamlListView>
    //      ...
    // </local:XamlListView>
    // Ok, but why ?
    public class XamlListView : ListView
    {
    }
    public class XamlGridView : GridView
    {
    }

    internal struct TransformBase
    {
        public bool IsMove;
        public TransformerMode Mode;

        public Transformer StartingTransformer;
        public Transformer Transformer;
    }
    internal struct Transform
    {
        public bool IsMove;
        public TransformerMode Mode;

        public Matrix3x2 Matrix;
        public TransformerBorder Border;

        public Transformer StartingTransformer;
        public Transformer Transformer;
    }
    internal struct FreeTransform
    {
        public Vector2 Distance;
        public TransformerMode Mode;

        public Matrix3x2 Matrix;
        public TransformerBorder Border;

        public Transformer Transformer;
    }

    internal static class ContentExtensions
    {
        //@Attached
        public static OptionType GetText(TextBlock dp) => default;
        public static void SetText(TextBlock dp, OptionType value) => dp.Text = value.ToString();

        public static OptionType GetAccessKey(MenuFlyoutSeparator dp) => default;
        public static void SetAccessKey(MenuFlyoutSeparator dp, OptionType value) => dp.AccessKey = value.ToString();

        public static OptionType GetIcon(ContentControl dp) => dp.Content is OptionType value ? value : default;
        public static void SetIcon(ContentControl dp, OptionType value)
        {
            dp.Resources.Source = new Uri(value.GetResource());
            dp.Content = new ContentControl
            {
                Content = value,
                Template = value.GetTemplate(dp.Resources)
            };
        }

        public static OptionType GetIconWithToolTip(ContentControl dp) => ContentExtensions.GetIcon(dp);
        public static void SetIconWithToolTip(ContentControl dp, OptionType value)
        {
            ContentExtensions.SetIcon(dp, value);
            ToolTipService.SetToolTip(dp, new ToolTip
            {
                Content = value.ToString(),
                Style = App.Current.Resources["AppToolTipStyle"] as Style
            });
        }

        public static ElementType GetElementIcon(ContentControl dp) => dp.Content is ElementType value ? value : default;
        public static void SetElementIcon(ContentControl dp, ElementType value)
        {
            dp.Resources.Source = new Uri(value.GetResource());
            dp.Content = new ContentControl
            {
                Content = value,
                Template = value.GetTemplate(dp.Resources)
            };
        }

        public static ElementType GetElementIconWithToolTip(ContentControl dp) => ContentExtensions.GetElementIcon(dp);
        public static void SetElementIconWithToolTip(ContentControl dp, ElementType value)
        {
            ContentExtensions.SetElementIcon(dp, value);
            ToolTipService.SetToolTip(dp, new ToolTip
            {
                Content = value.ToString(),
                Style = App.Current.Resources["AppToolTipStyle"] as Style
            });
        }

        public static ElementType GetElementItem(ContentControl dp) => ContentExtensions.GetElementIcon(dp);
        public static void SetElementItem(ContentControl dp, ElementType value)
        {
            dp.Resources.Source = new Uri(value.GetResource());
            dp.Content = CreateItem(value, value.ToString(), value.GetTemplate(dp.Resources));
        }

        public static BlendEffectMode GetBlendItem(ContentControl dp) => dp.Content is BlendEffectMode value ? value : default;
        public static void SetBlendItem(ContentControl dp, BlendEffectMode value)
        {
            dp.Content = CreateItem(value.GetIcon(), value.GetTitle());
        }

        private static Grid CreateItem(object value, string text, ControlTemplate icon = null) => new Grid
        {
            Children =
            {
                new TextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                new ContentControl
                {
                    Content = value,
                    Template = icon,
                    Width = 32,
                    FontWeight = Windows.UI.Text.FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                }
            }
        };
    }

    internal sealed class BlendList : List<BlendEffectMode> { }
    internal sealed class BlendGroupingList : List<BlendEffectMode> { }
    internal class BlendItem : TItem<BlendEffectMode>
    {
        protected override void OnTypeChanged(BlendEffectMode value)
        {
            base.TextBlock.Text = value.GetTitle();
            base.Icon.Content = value.GetIcon();
        }
    }

    internal sealed class HardnessList : List<BrushEdgeHardness> { }
    internal sealed class HardnessIcon : TIcon<BrushEdgeHardness>
    {
        protected override void OnTypeChanged(BrushEdgeHardness value)
        {
            base.Content = value.ToString();
        }
    }

    internal sealed class InterpolationItem : TItem<CanvasImageInterpolation>
    {
        protected override void OnTypeChanged(CanvasImageInterpolation value)
        {
            base.TextBlock.Text = value.ToString();
            base.Icon.Content = value.ToString().First().ToString();
        }
    }

    internal sealed class OptionGroupingList : GroupingList<OptionGrouping, OptionType, OptionType> { }
    internal class OptionGrouping : Grouping<OptionType, OptionType> { }

    internal class OptionIcon : TIcon<OptionType>
    {
        protected override void OnTypeChanged(OptionType value)
        {
            base.Content = value;
            base.Resources.Source = new Uri(value.GetResource());
            base.Template = value.GetTemplate(base.Resources);
        }
    }
    internal sealed class OptionThumbnail : TIcon<OptionType>
    {
        protected override void OnTypeChanged(OptionType value)
        {
            base.Content = value.ToString();

            // https://docs.microsoft.com/en-us/windows/uwp/debug-test-perf/optimize-animations-and-media
            if (value.ExistThumbnail())
            {
                BitmapImage bitmap = new BitmapImage();
                base.Background = new ImageBrush
                {
                    ImageSource = bitmap
                };
                bitmap.UriSource = new Uri(value.GetThumbnail());
            }
        }
    }

    internal static class CommandParameterExtensions
    {
        //@Attached
        public static OptionType GetButtonBase(ButtonBase dp) => dp.CommandParameter is OptionType value ? value : default;
        public static void SetButtonBase(ButtonBase dp, OptionType value) => dp.CommandParameter = value;

        public static OptionType GetSplitButton(SplitButton dp) => dp.CommandParameter is OptionType value ? value : default;
        public static void SetSplitButton(SplitButton dp, OptionType value) => dp.CommandParameter = value;

        public static OptionType GetMenuFlyoutItem(MenuFlyoutItem dp) => dp.CommandParameter is OptionType value ? value : default;
        public static void SetMenuFlyoutItem(MenuFlyoutItem dp, OptionType value)
        {
            if (value.IsItemClickEnabled())
            {
                dp.Text = value.ToString();
                dp.CommandParameter = value;
            }
        }

        public static OptionType GetMenuFlyoutItemWidthIcon(MenuFlyoutItem dp) => CommandParameterExtensions.GetMenuFlyoutItem(dp);
        public static void SetMenuFlyoutItemWidthIcon(MenuFlyoutItem dp, OptionType value)
        {
            CommandParameterExtensions.SetMenuFlyoutItem(dp, value);
            
            if (value.ExistIcon())
            {
                dp.Resources.Source = new Uri(value.GetResource());
                dp.Tag = new ContentControl
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Template = value.GetTemplate(dp.Resources)
                };
            }

            if (value.HasPreview())
            {
                dp.KeyboardAcceleratorTextOverride = "•";
            }
            else if (value.HasMenu())
            {
                dp.KeyboardAcceleratorTextOverride = ">";
            }
        }
    }

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
}