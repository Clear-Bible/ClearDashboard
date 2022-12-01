using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Services;
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

        public string Text
        {
            get => Entity.Text ?? string.Empty;
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
            set => Set(ref _associations, value);
        }

        public NoteViewModelCollection Replies
        {
            get => _replies;
            set => Set(ref _replies, value);
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

        /// <summary>
        /// Gets whether this note is eligible to be sent to Paratext.
        /// </summary>
        public bool EnableParatextSend => ParatextSendNoteInformation != null;

        /// <summary>
        /// Gets or sets the Paratext information of the associated tokens, or null if the note is not eligible to be sent to Paratext.
        /// </summary>
        /// <remarks>
        /// In order for a note to be sent to Paratext:
        /// 
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same Paratext corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// </remarks>
        public ParatextSendNoteInformation? ParatextSendNoteInformation { get; set; }

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
