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

        public int SourceCorpusId { get; set; }
        public int TargetCorpusId { get; set; }

        public int? AlignmentTypeId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastGenerated { get; set; }
        public int Id { get; set; }

        public virtual Corpus SourceCorpus { get; set; }
        public virtual Corpus TargetCorpus { get; set; }
        public virtual ICollection<ParallelVerse> ParallelVerses { get; set; }
    }
}
