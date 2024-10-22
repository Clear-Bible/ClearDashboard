﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Controls.DockPanelSplitter
{
    /// <summary>
    /// Adapted from here: https://github.com/JVimes/DockPanelSplitter
    /// MIT license
    /// </summary>
    

    /// <summary>
    ///   Like <see cref="GridSplitter"/>, but for <see cref="DockPanel"/>
    ///   instead of <see cref="Grid"/>.
    /// </summary>
    public class DockPanelSplitter : Thumb
    {
        static readonly FrameworkElement targetNullObject = new FrameworkElement();
        static readonly DockPanel parentNullObject = new DockPanel();

        bool isHorizontal;
        bool isBottomOrRight;
        FrameworkElement target = targetNullObject;
        double? initialLength;
        double availableSpace;


        static DockPanelSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPanelSplitter), new FrameworkPropertyMetadata(typeof(DockPanelSplitter)));
        }

        /// <summary> </summary>
        public DockPanelSplitter()
        {
            Loaded += OnLoaded;
            MouseDoubleClick += OnMouseDoubleClick;
            DragStarted += OnDragStarted;
            DragDelta += OnDragDelta;
        }


        DockPanel Panel => Parent as DockPanel ?? parentNullObject;


        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(Parent is DockPanel))
                throw new InvalidOperationException($"{nameof(DockPanelSplitter)} must be directly in a DockPanel.");

            if (GetTargetOrDefault() == default)
                throw new InvalidOperationException($"{nameof(DockPanelSplitter)} must be directly after a FrameworkElement");
        }

        void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (initialLength != null)
                SetTargetLength(initialLength.Value);
        }

        void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            isHorizontal = GetIsHorizontal(this);
            isBottomOrRight = GetIsBottomOrRight();
            target = GetTargetOrDefault() ?? targetNullObject;
            initialLength ??= GetTargetLength();
            availableSpace = GetAvailableSpace();
        }

        void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var change = isHorizontal ? e.VerticalChange : e.HorizontalChange;
            if (isBottomOrRight) change = -change;

            var targetLength = GetTargetLength();
            var newTargetLength = targetLength + change;
            newTargetLength = Clamp(newTargetLength, 0, availableSpace);
            newTargetLength = Math.Round(newTargetLength);

            SetTargetLength(newTargetLength);
        }

        FrameworkElement? GetTargetOrDefault()
        {
            var children = Panel.Children.OfType<object>();
            var splitterIndex = Panel.Children.IndexOf(this);
            return children.ElementAtOrDefault(splitterIndex - 1) as FrameworkElement;
        }

        bool GetIsBottomOrRight()
        {
            var position = DockPanel.GetDock(this);
            return position == Dock.Bottom || position == Dock.Right;
        }

        double GetAvailableSpace()
        {
            var lastChild =
                Panel.LastChildFill ?
                Panel.Children.OfType<object>().Last() as FrameworkElement :
                null;

            var fixedChildren =
                from child in Panel.Children.OfType<FrameworkElement>()
                where GetIsHorizontal(child) == isHorizontal
                where child != target
                where child != lastChild
                select child;

            var panelLength = GetLength(Panel);
            var unavailableSpace = fixedChildren.Sum(c => GetLength(c));
            return panelLength - unavailableSpace;
        }

        void SetTargetLength(double length)
        {
            if (isHorizontal) target.Height = length;
            else target.Width = length;
        }

        double GetTargetLength() => GetLength(target);

        static bool GetIsHorizontal(FrameworkElement element)
        {
            var position = DockPanel.GetDock(element);
            return GetIsHorizontal(position);
        }

        static bool GetIsHorizontal(Dock position)
            => position == Dock.Top || position == Dock.Bottom;

        static double Clamp(double value, double min, double max)
            => value < min ? min :
               value > max ? max :
               value;

        double GetLength(FrameworkElement element)
            => isHorizontal ?
               element.ActualHeight :
               element.ActualWidth;


        internal class CursorConverter : IValueConverter
        {
            public static CursorConverter Instance { get; } = new CursorConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var position = (Dock)value;
                var isHorizontal = GetIsHorizontal(position);
                return isHorizontal ? Cursors.SizeNS : Cursors.SizeWE;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
    }
}
