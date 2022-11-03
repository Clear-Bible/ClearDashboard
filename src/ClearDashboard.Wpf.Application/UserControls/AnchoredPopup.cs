using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace ClearDashboard.Wpf.Application.UserControls
{

    internal class AnchoredPopup : Popup
    {
        private Window _root;

        public AnchoredPopup()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _root = Window.GetWindow(this)!;
            _root.LocationChanged += OnRootLocationChanged;

        }

        private void OnRootLocationChanged(object? sender, EventArgs e)
        {
            var offset = this.HorizontalOffset;
            this.HorizontalOffset = offset + 1;
            this.HorizontalOffset = offset;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _root.LocationChanged -= OnRootLocationChanged;
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
    }
}
