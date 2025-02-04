﻿using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter.Layers
{
    public static class BlendExtensions
    {

        public static BlendEffectMode None = (BlendEffectMode)(-1); // None
        public static bool IsDefined(this BlendEffectMode type) => System.Enum.IsDefined(typeof(BlendEffectMode), type);

        public static string GetTitle(this BlendEffectMode type) => type.IsDefined() ? type.ToString() : "None";
        public static string GetIcon(this BlendEffectMode type) => type.IsDefined() ? type.ToString().First().ToString() : "N";

        //@Resource
        public static string GetResource(this BlendEffectMode type) => $"ms-appx:///Luo Painter.Blends/Icons/{type.GetTitle()}Icon.xaml";
        public static ControlTemplate GetTemplate(this BlendEffectMode type, ResourceDictionary resource) => resource[$"{type.GetTitle()}Icon"] as ControlTemplate;
        public static ControlTemplate GetTemplate(this BlendEffectMode type, out ResourceDictionary resource, out string title)
        {
            title = type.GetTitle();
            resource = new ResourceDictionary
            {
                Source = new Uri($"ms-appx:///Luo Painter.Blends/Icons/{title}Icon.xaml")
            };
            return resource[$"{title}Icon"] as ControlTemplate;
        }

    }
}