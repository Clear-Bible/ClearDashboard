using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteUpdatedMessage(NoteId NoteId, bool succeeded, string NoteStatus = null);
}
