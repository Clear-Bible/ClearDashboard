using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelVerse
    {
        public long Id { get; set; }
        public long SourceVerseId { get; set; }
        public long TargetVerseId { get; set; }
        public long? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus ParallelCorpus { get; set; }
        public virtual Verse VerseVerse1 { get; set; }
        public virtual Verse VerseVerseNavigation { get; set; }
    }
}
