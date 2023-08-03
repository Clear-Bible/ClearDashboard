using System.Windows;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelGroupLabelEventArgs : LabelGroupEventArgs
    {
        /// <summary>
        /// The <see cref="Label"/> to which the label group operation applies.
        /// </summary>
        public Label Label { get; set; } = new();
    }
}
