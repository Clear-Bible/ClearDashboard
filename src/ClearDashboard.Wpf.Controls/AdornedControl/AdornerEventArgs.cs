using System.Windows;

namespace ClearDashboard.Wpf.Controls
{
    public class AdornerEventArgs : RoutedEventArgs
    {
        public AdornerEventArgs(RoutedEvent routedEvent, object source, FrameworkElement adorner) :
            base(routedEvent, source)
        {
            Adorner = adorner;
        }

        public FrameworkElement Adorner { get; } = null;
    }

    public delegate void AdornerEventHandler(object sender, AdornerEventArgs e);
}
