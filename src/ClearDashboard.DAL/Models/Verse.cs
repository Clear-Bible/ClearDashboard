using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Verse
    {

        public Verse()
        {
            ParallelVersesLinks = new HashSet<ParallelVersesLink>();
        }
        public int Id { get; set; }

        // Add unique constraint for VerseNumber, SilBookNumber and ChapterNumber
        public string VerseNumber { get; set; }
        public string SilBookNumber { get; set; }
        public string ChapterNumber { get; set; }

        public string VerseText { get; set; }
        public DateTime LastChanged { get; set; }
        public int? CorpusId { get; set; }

        public virtual Corpus Corpus { get; set; }
        public virtual Token Token { get; set; }
        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }
    }
}
