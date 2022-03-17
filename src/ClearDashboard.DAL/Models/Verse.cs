using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Verse
    {
        public long Id { get; set; }
        public string BookId { get; set; }
        public string VerseText { get; set; }
        public byte[] LastChanged { get; set; }
        public long? CorpusId { get; set; }

        public virtual Corpus Corpus { get; set; }
        public virtual ParallelVerse Verse1 { get; set; }
        public virtual Token Verse2 { get; set; }
        public virtual ParallelVerse VerseNavigation { get; set; }
    }
}
