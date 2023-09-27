using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelGroupSelectedEventArgs : LabelGroupEventArgs
    {
        public UserId UserId { get; set; }
    }
}
