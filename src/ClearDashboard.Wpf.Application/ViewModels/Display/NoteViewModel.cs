using System.Collections.ObjectModel;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class NoteViewModel : PropertyChangedBase
    {
        public Note Note { get; set; }

        public NoteId? NoteId => Note.NoteId;

        public string? Text
        {
            get => Note.Text;
            set
            {
                Note.Text = value;
                NotifyOfPropertyChange();
            }
        }

        public string NoteStatus
        {
            get => Note.NoteStatus;
            set
            {
                Note.NoteStatus = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Label> Labels
        {
            get => Note.Labels;
            set
            {
                Note.Labels = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was created.
        /// </summary>
        public string Created => Note.NoteId?.Created != null ? Note.NoteId.Created.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified.
        /// </summary>
        public string Modified => Note.NoteId != null && Note.NoteId.Modified != null ? Note.NoteId.Modified.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets the display name of the user that last modified the note.
        /// </summary>
        public string ModifiedBy => (Note.NoteId != null && Note.NoteId.UserId != null ? Note.NoteId.UserId.DisplayName : string.Empty) ?? string.Empty;

        public NoteViewModel() : this(new Note())
        {
        }

        public NoteViewModel(Note note)
        {
            Note = note;
        }
    }
}
