using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelVerse
    {
        public int Id { get; set; }
        public int SourceVerseId { get; set; }
        public int TargetVerseId { get; set; }
        public int? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus ParallelCorpus { get; set; }
        public virtual Verse VerseVerse1 { get; set; }
        public virtual Verse VerseVerseNavigation { get; set; }
    }
}
