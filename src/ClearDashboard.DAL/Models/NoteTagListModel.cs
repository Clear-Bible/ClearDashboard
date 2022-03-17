using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace ClearDashboard.DataAccessLayer.Models
{
    /// <summary>
    /// Contains the list of notes and tags as well as queries for building the db table and basic select query.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class NoteTagListModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor needed for serialization.
        /// </summary>
        public NoteTagListModel() { }

        private ObservableCollection<NoteTagModel> _notesAndTags = new();
        /// <summary>
        /// List of Notes and Tags
        /// </summary>
        public ObservableCollection<NoteTagModel> NotesAndTags
        {
            get { return _notesAndTags; }
            set
            {
                _notesAndTags = value;
                OnPropertyChanged();
            }
        }

        private string _projectName;

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                _projectName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns a list of Notes from an already loaded list.
        /// </summary>
        /// <returns></returns>
        public List<NoteTagModel> Notes()
        {
            return NotesAndTags.Where(w => w.NoteOrTag == ENums.NoteTagType.Note).ToList();
        }

        /// <summary>
        /// Returns a list of Tags from an already loaded list.
        /// </summary>
        /// <returns></returns>
        public List<NoteTagModel> Tags()
        {
            return NotesAndTags.Where(w => w.NoteOrTag == ENums.NoteTagType.Tag).ToList();
        }

        // TODO Replace this with whatever BCV object we are using.
        //public List<NoteTagModel> TagsOnThisVerse(string verseRef)
        //{
        //    return Tags().Where(w => w.VerseReferences.Any(v => v.VerseId == verseRef)).ToList();
        //}

        //public List<NoteTagModel> NotesOnThisVerse(string verseRef)
        //{
        //    return Notes().Where(w => w.VerseReferences.Any(v => v.VerseId == verseRef)).ToList();
        //}

        /// <summary>
        /// Triggers an update of the UI when the data changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}