using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Verse
    {
        public int Id { get; set; }
        public string BookId { get; set; }
        public string VerseText { get; set; }
        public DateTime LastChanged { get; set; }
        public int? CorpusId { get; set; }

        public virtual Corpus Corpus { get; set; }
        public virtual ParallelVerse Verse1 { get; set; }
        public virtual Token Verse2 { get; set; }
        public virtual ParallelVerse VerseNavigation { get; set; }
    }
}
