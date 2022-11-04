using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ClearDashboard.Wpf.Application.UserControls
{

    internal class AnchoredPopupOld : Popup
    {
        private Window? _parentWindow;

        public AnchoredPopupOld()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            //var baseType = GetType().BaseType;
            //var popupSecHelper = GetHiddenField(this, baseType!, "_secHelper");
            //SetHiddenField(popupSecHelper, "_isChildPopupInitialized", true);
            //SetHiddenField(popupSecHelper, "_isChildPopup", true);

            //this.AllowsTransparency = true;
            //Background = System.Windows.Media.Brushes.Transparent
        }

        protected dynamic? GetHiddenField(object container, string fieldName)
        {
            return GetHiddenField(container, container.GetType(), fieldName);
        }

        protected dynamic? GetHiddenField(object container, Type containerType, string fieldName)
        {
            dynamic? retVal = null;
            var fieldInfo = containerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                retVal = fieldInfo.GetValue(container)!;
            }
            return retVal;
        }

        protected void SetHiddenField(object container, string fieldName, object value)
        {
            SetHiddenField(container, container.GetType(), fieldName, value);
        }

        protected void SetHiddenField(object container, Type containerType, string fieldName, object value)
        {
            var fieldInfo = containerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(container, value);
            }
        }



        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this)!;
            _parentWindow.LocationChanged += OnParentWindowLocationChanged;
            _parentWindow.SizeChanged += OnParentWindowSizeChanged;

        }

        private void OnParentWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveWithParent();
        }


        // Hack to force the pop up window to move with it's parent window.
        private void OnParentWindowLocationChanged(object? sender, EventArgs e)
        {
            MoveWithParent();
        }

        private void MoveWithParent()
        {
            var horizontalOffset = HorizontalOffset;
            HorizontalOffset = horizontalOffset + 1;
            HorizontalOffset = horizontalOffset;

            var verticalOffset = VerticalOffset;
            VerticalOffset = verticalOffset + 1;
            VerticalOffset = verticalOffset;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _parentWindow!.LocationChanged -= OnParentWindowLocationChanged;
            _parentWindow!.SizeChanged -= OnParentWindowSizeChanged;
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
    }
}
