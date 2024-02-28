using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteUpdatedMessage(Note Note, bool succeeded);
}
