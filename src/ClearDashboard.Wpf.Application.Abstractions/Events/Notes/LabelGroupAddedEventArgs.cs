using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelGroupAddedEventArgs : LabelGroupEventArgs
    {
        /// <summary>
        /// The source <see cref="LabelGroupViewModel"/> on which to base the new label group.
        /// </summary>
        public LabelGroupViewModel? SourceLabelGroup { get; set; }
    }
}
