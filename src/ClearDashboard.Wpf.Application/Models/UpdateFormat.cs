using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class UpdateFormat
    {
        public string Version { get; set; } = String.Empty;
        public string ReleaseDate { get; set; } = String.Empty;
        public List<ReleaseNote> ReleaseNotes { get; set; } = new();
        public string DownloadLink { get; set; } = String.Empty;
    }

    public class ReleaseNote
    {
        public enum ReleaseNoteType
        {
            Added,
            BugFix,
            Changed,
            Fixed,
            Updated,
            Deferred
        }

        public ReleaseNoteType NoteType { get; set; } = ReleaseNoteType.Added;
        public string Note { get; set; } = String.Empty;
    }
}
