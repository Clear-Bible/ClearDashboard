using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelCorpus
    {
        public ParallelCorpus()
        {
            ParallelVerses = new HashSet<ParallelVerse>();
        }

        public long SourceCorpusId { get; set; }
        public long TargetCorpusId { get; set; }

        public long? AlignmentType { get; set; }
        public byte[] CreationDate { get; set; }
        public byte[] LastGenerated { get; set; }
        public long Id { get; set; }

        public virtual Corpus CorpusCorpus { get; set; }
        public virtual Corpus CorpusCorpusNavigation { get; set; }
        public virtual ICollection<ParallelVerse> ParallelVerses { get; set; }
    }
}
