﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Luo_Painter.Controls
{
    public sealed class NumberButton : NumberButtonBase, INumberSlider
    {
        //@Content
        public FrameworkElement PlacementTarget => this;

        public int Number
        {
            get => this.number;
            set
            {
                this.number = value;
                if (string.IsNullOrEmpty(this.Unit)) base.Content = value;
                else base.Content = $"{value} {this.Unit}";
            }
        }
        private int number;
        public int NumberMinimum { get; set; }
        public int NumberMaximum { get; set; } = 100;

        public string Unit
        {
            get => this.unit;
            set
            {
                this.unit = value;
                if (string.IsNullOrEmpty(this.Unit)) base.Content = this.Number;
                else base.Content = $"{this.Number} {value}";
            }
        }
        private string unit = string.Empty;

    }

    public partial class NumberButtonBase : Button
    {
        //@Construct
        public NumberButtonBase()
        {
            this.InitializeComponent();
        }
    }
}