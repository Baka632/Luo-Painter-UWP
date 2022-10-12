﻿using FanKit.Transformers;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Input;

namespace Luo_Painter.Brushes
{
    /// <summary>
    /// The input device type of <see cref = "CanvasOperator" />.
    /// </summary>
    internal enum InkInputDevice
    {
        None,

        Holding,
        OneFinger,
        DoubleFinger,

        Pen,
        Eraser,

        LeftButton,
        RightButton,
    }

    /// <summary> 
    /// Provides single-finger, double-finger, mobile events to pointer events for canvas controls.
    /// </summary>
    public class InkCanvasOperator : DependencyObject
    {


        #region DependencyProperty


        /// <summary>
        /// <see cref = "InkCanvasOperator" />'s destination control.
        /// </summary>
        public UIElement DestinationControl
        {
            get => (UIElement)base.GetValue(DestinationControlProperty);
            set => base.SetValue(DestinationControlProperty, value);
        }
        /// <summary> Identifies the <see cref = "InkCanvasOperator.DestinationControl" /> dependency property. </summary>
        public static DependencyProperty DestinationControlProperty = DependencyProperty.Register(nameof(DestinationControl), typeof(UIElement), typeof(InkCanvasOperator), new PropertyMetadata(null, (sender, e) =>
        {
            InkCanvasOperator control = (InkCanvasOperator)sender;

            if (e.OldValue is UIElement oldValue)
            {
                oldValue.PointerPressed -= control.Control_PointerPressed;
                oldValue.PointerReleased -= control.Control_PointerReleased;
                oldValue.PointerCanceled -= control.Control_PointerReleased;
                oldValue.PointerMoved -= control.Control_PointerMoved;
                oldValue.PointerWheelChanged -= control.Control_PointerWheelChanged;
            }

            if (e.NewValue is UIElement value)
            {
                value.PointerPressed += control.Control_PointerPressed;
                value.PointerReleased += control.Control_PointerReleased;
                value.PointerCanceled += control.Control_PointerReleased;
                value.PointerMoved += control.Control_PointerMoved;
                value.PointerWheelChanged += control.Control_PointerWheelChanged;
            }
        }));


        #endregion


        #region Delegate


        //@Delegate
        /// <summary> Occurs when the one-finger | mouse-left-button | pen event starts. </summary>
        public event SingleHandler Single_Start = null;
        /// <summary> Occurs when one-finger | mouse-left-button | pen event change. </summary>
        public event SingleHandler Single_Delta = null;
        /// <summary> Occurs when the one-finger | mouse-left-button | pen event is complete. </summary>
        public event SingleHandler Single_Complete = null;


        /// <summary> Occurs when the mouse-right-button event starts. </summary>
        public event RightHandler Right_Start = null;
        /// <summary> Occurs when mouse-right-button event change. </summary>
        public event RightHandler Right_Delta = null;
        /// <summary> Occurs when the mouse-right-button event is complete. </summary>
        public event RightHandler Right_Complete = null;


        /// <summary> Occurs when the double-finger event starts. </summary>
        public event DoubleHandler Double_Start = null;
        /// <summary> Occurs when double-finger event change. </summary>
        public event DoubleHandler Double_Delta = null;
        /// <summary> Occurs when the double-finger event is complete. </summary>
        public event DoubleHandler Double_Complete = null;


        /// <summary>
        /// Occurs when the incremental value of the pointer wheel changes.
        /// </summary>
        public event DoubleHandler Wheel_Changed = null;


        #endregion


        #region Point


        uint PointerId;
        uint EvenPointerId;
        uint OddPointerId;

        Vector2 StartingEvenPoint;
        Vector2 EvenPoint;
        Vector2 OddPoint;

        InkInputDevice Device;


        // Pointer Pressed
        private void Control_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            switch (e.Pointer.PointerDeviceType)
            {
                case PointerDeviceType.Touch:
                    if (this.EvenPointerId == default)
                    {
                        this.EvenPointerId = e.Pointer.PointerId;

                        PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                        this.StartingEvenPoint = this.EvenPoint = pointerPoint.Position.ToVector2();

                        this.Device = InkInputDevice.Holding;
                        return;
                    }
                    else if (this.OddPointerId == default)
                    {
                        this.OddPointerId = e.Pointer.PointerId;

                        PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                        this.OddPoint = pointerPoint.Position.ToVector2();

                        this.Device = InkInputDevice.DoubleFinger;
                        this.DestinationControl.CapturePointer(e.Pointer);
                        this.Double_Start?.Invoke((this.OddPoint + this.EvenPoint) / 2, Vector2.Distance(this.OddPoint, this.EvenPoint)); // Delegate
                        return;
                    }
                    else
                    {
                        return;
                    }
                case PointerDeviceType.Pen:
                    if (this.PointerId == default)
                    {
                        this.PointerId = e.Pointer.PointerId;

                        PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                        Vector2 point = pointerPoint.Position.ToVector2();

                        if (pointerPoint.Properties.IsEraser)
                        {
                            this.Device = InkInputDevice.Eraser;
                            this.DestinationControl.CapturePointer(e.Pointer);
                            this.Single_Start?.Invoke(point, pointerPoint.Properties); // Delegate
                            return;
                        }
                        else
                        {
                            this.Device = InkInputDevice.Pen;
                            this.DestinationControl.CapturePointer(e.Pointer);
                            this.Single_Start?.Invoke(point, pointerPoint.Properties); // Delegate
                            return;
                        }
                    }
                    else
                    {
                        this.Device = InkInputDevice.None;
                        return;
                    }
                case PointerDeviceType.Mouse:
                    if (this.PointerId == default)
                    {
                        this.PointerId = e.Pointer.PointerId;

                        PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                        Vector2 point = pointerPoint.Position.ToVector2();

                        if (pointerPoint.Properties.IsRightButtonPressed)
                        {
                            this.Device = InkInputDevice.RightButton;
                            this.DestinationControl.CapturePointer(e.Pointer);
                            this.Right_Start?.Invoke(point); // Delegate
                            return;
                        }
                        else
                        {
                            this.Device = InkInputDevice.LeftButton;
                            this.DestinationControl.CapturePointer(e.Pointer);
                            this.Single_Start?.Invoke(point, pointerPoint.Properties); // Delegate
                            return;
                        }
                    }
                    else
                    {
                        this.Device = InkInputDevice.None;
                        return;
                    }
                default:
                    this.Device = InkInputDevice.None;
                    return;
            }
        }
        // Pointer Released
        private void Control_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.PointerId = default;
            this.EvenPointerId = default;
            this.OddPointerId = default;

            PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
            Vector2 point = pointerPoint.Position.ToVector2();

            this.DestinationControl.ReleasePointerCaptures();
            switch (this.Device)
            {
                case InkInputDevice.OneFinger:
                    this.Single_Complete?.Invoke(point, pointerPoint.Properties); // Delegate
                    break;
                case InkInputDevice.DoubleFinger:
                    this.Double_Complete?.Invoke((this.OddPoint + this.EvenPoint) / 2, Vector2.Distance(this.OddPoint, this.EvenPoint)); // Delegate
                    break;
                case InkInputDevice.Pen:
                    this.Single_Complete?.Invoke(point, pointerPoint.Properties); // Delegate
                    break;
                case InkInputDevice.Eraser:
                    this.Single_Complete?.Invoke(point, pointerPoint.Properties); // Delegate
                    break;
                case InkInputDevice.LeftButton:
                    this.Single_Complete?.Invoke(point, pointerPoint.Properties); // Delegate
                    break;
                case InkInputDevice.RightButton:
                    this.Right_Complete?.Invoke(point); // Delegate
                    break;
                default:
                    break;
            }

            this.Device = default;
        }


        // Pointer Moved
        private void Control_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            switch (e.Pointer.PointerDeviceType)
            {
                case PointerDeviceType.Touch:
                    switch (this.Device)
                    {
                        case InkInputDevice.Holding:
                            {
                                PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                                if (this.EvenPointerId == e.Pointer.PointerId)
                                {
                                    this.EvenPoint = pointerPoint.Position.ToVector2();

                                    if (Vector2.Distance(this.StartingEvenPoint, this.EvenPoint) > 12f)
                                    {
                                        this.Device = InkInputDevice.OneFinger;
                                        this.DestinationControl.CapturePointer(e.Pointer);
                                        this.Single_Start?.Invoke(this.EvenPoint, pointerPoint.Properties); // Delegate
                                    }
                                }
                                return;
                            }
                        case InkInputDevice.OneFinger:
                            {
                                PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);

                                if (this.EvenPointerId == e.Pointer.PointerId)
                                {
                                    this.EvenPoint = pointerPoint.Position.ToVector2();
                                }

                                this.Single_Delta?.Invoke(this.EvenPoint, pointerPoint.Properties); // Delegate
                                return;
                            }
                        case InkInputDevice.DoubleFinger:
                            {
                                PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);

                                if (this.EvenPointerId == e.Pointer.PointerId)
                                {
                                    this.EvenPoint = pointerPoint.Position.ToVector2();
                                }
                                if (this.OddPointerId == e.Pointer.PointerId)
                                {
                                    this.OddPoint = pointerPoint.Position.ToVector2();
                                }

                                this.Double_Delta?.Invoke((this.OddPoint + this.EvenPoint) / 2, Vector2.Distance(this.OddPoint, this.EvenPoint)); // Delegate
                                return;
                            }
                        default:
                            return;
                    }
                case PointerDeviceType.Pen:
                    switch (this.Device)
                    {
                        case InkInputDevice.Pen:
                            if (this.PointerId != default)
                            {
                                if (this.PointerId == e.Pointer.PointerId)
                                {
                                    PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                                    Vector2 point = pointerPoint.Position.ToVector2();

                                    this.Single_Delta?.Invoke(point, pointerPoint.Properties); // Delegate
                                }
                            }
                            return;
                        case InkInputDevice.Eraser:
                            if (this.PointerId != default)
                            {
                                if (this.PointerId == e.Pointer.PointerId)
                                {
                                    PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                                    Vector2 point = pointerPoint.Position.ToVector2();

                                    this.Single_Delta?.Invoke(point, pointerPoint.Properties); // Delegate
                                }
                            }
                            return;
                        default:
                            return;
                    }
                case PointerDeviceType.Mouse:
                    switch (this.Device)
                    {
                        case InkInputDevice.LeftButton:
                            if (this.PointerId != default)
                            {
                                if (this.PointerId == e.Pointer.PointerId)
                                {
                                    PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                                    Vector2 point = pointerPoint.Position.ToVector2();

                                    this.Single_Delta?.Invoke(point, pointerPoint.Properties); // Delegate
                                }
                            }
                            return;
                        case InkInputDevice.RightButton:
                            if (this.PointerId != default)
                            {
                                if (this.PointerId == e.Pointer.PointerId)
                                {
                                    PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);
                                    Vector2 point = pointerPoint.Position.ToVector2();

                                    this.Right_Delta?.Invoke(point); // Delegate
                                }
                            }
                            return;
                        default:
                            return;
                    }
                default:
                    return;
            }
        }
        // Wheel Changed
        private void Control_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pointerPoint = e.GetCurrentPoint(this.DestinationControl);

            Vector2 point = pointerPoint.Position.ToVector2();
            float space = pointerPoint.Properties.MouseWheelDelta;

            this.Wheel_Changed?.Invoke(point, space);//Delegate
        }


        #endregion

    }
}