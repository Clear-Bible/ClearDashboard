using System;
using ClearDashboard.DataAccessLayer.Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClearDashboard.DataAccessLayer.Models
{
    /// <summary>
    /// Data Model for the Note Tag objects.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class NoteTagModel : DataObject
    {
        /// <summary>
        /// Default constructor needed for serialization.
        /// </summary>
        public NoteTagModel(){}

        
        private ENums.NoteTagType noteTagType = ENums.NoteTagType.Note;
        /// <summary>
        /// What type of object is this?
        /// The difference is in where in the UI it shows up.
        /// For Tags, the Title is in the Tag list.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ENums.NoteTagType NoteOrTag
        {
            get { return noteTagType; }
            set
            {
                noteTagType = value;
                OnPropertyChanged();
            }
        }

        private string _Title = string.Empty;
        /// <summary>
        /// The title of the Tag or Note. This is the Tag abbr or Note title.
        /// </summary>
        [JsonProperty]
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }

        private string _text = string.Empty;
        /// <summary>
        /// Used in the note only. Formerly named Note.
        /// </summary>
        [JsonProperty]
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        private string _English = string.Empty;
        /// <summary>
        /// The English word the Tag or note is about.
        /// </summary>
        [JsonProperty]
        public string English
        {
            get { return _English; }
            set
            {
                _English = value;
                OnPropertyChanged();
            }
        }

        private string _AltText = string.Empty;
        /// <summary>
        /// The alternate text the Tag or note is about.
        /// </summary>
        [JsonProperty]
        public string AltText
        {
            get { return _AltText; }
            set
            {
                _AltText = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset _LastChangedOffset = DateTimeOffset.UtcNow;
        [JsonProperty]
        [JsonConverter(typeof(EsDateTimeOffsetConverter))]
        public DateTimeOffset LastChangedOffset
        {
            get { return _LastChangedOffset; }
            set
            {
                _LastChangedOffset = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// For storing and getting from the database.
        /// From Unix Time Seconds
        /// </summary>
        [JsonProperty]
        public string LastChanged
        {
            get { return LastChangedOffset.ToUnixTimeSeconds().ToString(); }
            set
            {
                long unixSeconds = long.Parse(value);
                LastChangedOffset = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }
        }

        // TODO Replace this with whatever BCV object we are using.
        //private List<VerseInfoDataObject> _VerseReferences = new List<VerseInfoDataObject>();
        ///// <summary>
        ///// The verse "Book Chapter Verse" location. Example: Numbers 3:1
        ///// </summary>
        //[JsonProperty]
        //[JsonConverter(typeof(VerseInfoDataObjectConverter))]
        //public List<VerseInfoDataObject> VerseReferences
        //{
        //    get { return _VerseReferences; }
        //    set
        //    {
        //        _VerseReferences = value;
        //        OnPropertyChanged();
        //    }
        //}

        // TODO Replace this with whatever BCV object we are using.

        /// <summary>
        /// Returns a string representing the class object as a string.
        /// </summary>
        /// <returns>A string formatted version of this object.</returns>
        //public override string ToString()
        //{
        //    string note = string.Concat(Title, " ", Text);

        //    if(!string.IsNullOrWhiteSpace(English)) note += string.Concat("\r\n\tEnglish: ", English);
        //    if (!string.IsNullOrWhiteSpace(AltText)) note += string.Concat("\r\n\tAlt: ", AltText);

        //    string verseRef = "\r\nVerses:";

        //    VerseReferences.ForEach(n => { verseRef = string.Concat(verseRef, "\r\n\t", n); });

        //    return string.Concat(note, verseRef);
        //}

        //[OnDeserialized]
        //private void OnDeserialized(StreamingContext context)
        //{
        //    string VerseReferences = string.Join(",", _VerseReferences);
        //}

        public NoteTagModel ShallowCopy()
        {
            return (NoteTagModel)this.MemberwiseClone();
        }
    }
}
