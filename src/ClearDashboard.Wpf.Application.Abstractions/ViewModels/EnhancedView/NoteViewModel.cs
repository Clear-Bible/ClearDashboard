using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using TimeZoneNames;
using static SIL.Scripture.MultilingScrBooks;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class NoteViewModel : PropertyChangedBase
    {
        private NoteAssociationViewModelCollection _associations = new();
        private NoteViewModelCollection _replies = new();

        public Note Entity { get; set; }
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
                if (Equals(value, Entity.NoteStatus))
                    return;
                Entity.NoteStatus = value;
                NotifyOfPropertyChange();
            }
        }

        private LabelCollection? _labels;
        public LabelCollection Labels
        {
            get => _labels ??= new LabelCollection(Entity.Labels);
            set
            {
                Entity.Labels = value;
                _labels = new LabelCollection(Entity.Labels);
                NotifyOfPropertyChange();
            }
        }

        public NoteAssociationViewModelCollection Associations
        {
            get => _associations;
            set => Set(ref _associations, value);
        }


        public string AssociationVerse
        {
            get
            {
                string verses = String.Empty;
                foreach (var association in Associations)
                {
                    try
                    {
                        var verseId = string.Empty;
                        if (association.AssociatedEntityId is TranslationId translationId)
                        {
                            verseId = translationId.SourceTokenId.ToString().Substring(0, 9);
                        }
                        else
                        {
                            verseId = association.AssociatedEntityId.ToString().Substring(0, 9);
                        }

                        if (verseId.Length > 0)
                        {
                            verses += DAL.ViewModels.BookChapterVerseViewModel.GetVerseStrShortFromBBBCCCVVV(verseId) + ", ";
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                }

                if (verses.Length > 2)
                {
                    verses = verses.Substring(0, verses.Length - 2);
                }

                return verses;
            }
        }



        public NoteViewModelCollection Replies
        {
            get => _replies;
            set => Set(ref _replies, value);
        }

        public ICollection<Guid> SeenByUserIds
        {
            get => Entity.SeenByUserIds;
        }

        public void AddSeenByUserId(Guid userId)
        {
            Entity.SeenByUserIds.Add(userId);
            NotifyOfPropertyChange(nameof(SeenByUserIds));
        }

        public void RemoveSeenByUserId(Guid userId)
        {
            Entity.SeenByUserIds.Remove(userId);
            NotifyOfPropertyChange(nameof(SeenByUserIds));
        }

        /// <summary>
        /// returns the collection of seenbyuserids that has seen every reply (Intersection)
        /// or null if there are no replies
        /// </summary>
        public ICollection<Guid>? UserIdsSeenAllReplies
        {
            get
            {
                if (Replies == null || Replies.Count() == 0)
                    return null;

                return Replies
                    .Select(rnvm => rnvm.SeenByUserIds)
                    .Skip(1)
                    .Aggregate(
                        Replies.Count() > 0 ?
                            new HashSet<Guid>(Replies.First().SeenByUserIds) :
                            new HashSet<Guid>(),
                        (intersection, next) =>
                        {
                            intersection.IntersectWith(next);
                            return intersection;
                        }
                    );
            }
        }


        /// <summary>
        /// Gets a formatted string corresponding to the date the note was created.
        /// </summary>
        public string Created => Entity.NoteId?.Created != null ? Entity.NoteId.Created.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was created, converted to local time.
        /// </summary>
        public string CreatedLocalTime
        {
            get
            {
                var localTimeZone = TimeZoneInfo.Local;
                var localTimeZoneAbbreviations = TZNames.GetAbbreviationsForTimeZone(localTimeZone.Id, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                var localTimeZoneAbbreviation = localTimeZone.IsDaylightSavingTime(DateTime.Now) ? localTimeZoneAbbreviations.Daylight : localTimeZoneAbbreviations.Standard;
                var localTime = Entity?.NoteId?.Created?.ToLocalTime() ?? null;

                return localTime != null ? $"{localTime.Value:d} {localTime.Value:hhmm} {localTimeZoneAbbreviation}" : string.Empty;
            }
        }

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified.
        /// </summary>
        public string Modified => Entity.NoteId != null && Entity.NoteId.Modified != null ? Entity.NoteId.Modified.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified, converted to local time.
        /// </summary>
        public string ModifiedLocalTime
        {
            get
            {
                var localTimeZone = TimeZoneInfo.Local;
                var localTimeZoneAbbreviations = TZNames.GetAbbreviationsForTimeZone(localTimeZone.Id, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                var localTimeZoneAbbreviation = localTimeZone.IsDaylightSavingTime(DateTime.Now) ? localTimeZoneAbbreviations.Daylight : localTimeZoneAbbreviations.Standard;
                var localTime = Entity?.NoteId?.Modified?.ToLocalTime() ?? null;

                return localTime != null ? $"{localTime.Value:d} {localTime.Value:hhmm} {localTimeZoneAbbreviation}" : string.Empty;
            }
        }

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
        public ExternalSendNoteInformation? ParatextSendNoteInformation { get; set; }
        

        private bool _isSelectedForBulkAction = false;
        public bool IsSelectedForBulkAction
        {
            get => _isSelectedForBulkAction;
            set
            {
                _isSelectedForBulkAction = value;
                NotifyOfPropertyChange(() => IsSelectedForBulkAction);
            }
        }

        public void NoteSeenChanged()
        {
            NotifyOfPropertyChange(nameof(SeenByUserIds));
        }

        public NoteViewModel() : this(new Note())
        {
        }

        public NoteViewModel(Note note)
        {
            Entity = note;
            _labels = new LabelCollection(note.Labels);
            Associations = new NoteAssociationViewModelCollection();
            Replies = new NoteViewModelCollection();
        }
    }
}
