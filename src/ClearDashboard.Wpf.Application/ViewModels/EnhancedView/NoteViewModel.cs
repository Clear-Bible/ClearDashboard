using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.Collections;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class NoteViewModel : PropertyChangedBase
    {
        private NoteAssociationViewModelCollection _associations = new();
        private NoteViewModelCollection _replies = new();

        public Note Entity { get; }
        public NoteId? NoteId => Entity.NoteId;
        public EntityId<NoteId>? ThreadId => Entity.ThreadId;

        public string? ParatextId { get; set; }
        public bool EnableParatextSend => !string.IsNullOrEmpty(ParatextId);

        public string? Text
        {
            get => Entity.Text;
            set
            {
                if (Equals(value, Entity.Text)) return;
                Entity.Text = value;
                NotifyOfPropertyChange();
            }
        }

        public string NoteStatus
        {
            get => Entity.NoteStatus;
            set
            {
                if (Equals(value, Entity.NoteStatus)) return;
                Entity.NoteStatus = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Label> Labels
        {
            get => Entity.Labels;
            set
            {
                if (Equals(value, Entity.Labels)) return;
                Entity.Labels = value;
                NotifyOfPropertyChange();
            }
        }

        public NoteAssociationViewModelCollection Associations
        {
            get => _associations;
            set
            {
                if (Equals(value, _associations)) return;
                _associations = value;
                NotifyOfPropertyChange();
            }
        }

        public NoteViewModelCollection Replies
        {
            get => _replies;
            set
            {
                if (Equals(value, _replies)) return;
                _replies = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was created.
        /// </summary>
        public string Created => Entity.NoteId?.Created != null ? Entity.NoteId.Created.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified.
        /// </summary>
        public string Modified => Entity.NoteId != null && Entity.NoteId.Modified != null ? Entity.NoteId.Modified.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets the display name of the user that last modified the note.
        /// </summary>
        public string ModifiedBy => (Entity.NoteId != null && Entity.NoteId.UserId != null ? Entity.NoteId.UserId.DisplayName : string.Empty) ?? string.Empty;

        public NoteViewModel() : this(new Note())
        {
        }

        public NoteViewModel(Note note)
        {
            Entity = note;
            Associations = new NoteAssociationViewModelCollection();
            Replies = new NoteViewModelCollection();
        }
    }
}
