using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelGroupEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="LabelGroupViewModel"/> to which the event pertains.
        /// </summary>
        public LabelGroupViewModel LabelGroup { get; set; } = new();
    }
}
