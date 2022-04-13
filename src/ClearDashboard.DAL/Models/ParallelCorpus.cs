using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelCorpus
    {
        public ParallelCorpus()
        {
            ParallelVersesLinks = new HashSet<ParallelVersesLink>();
        }

        public int SourceCorpusId { get; set; }
        public int TargetCorpusId { get; set; }

        public int? AlignmentTypeId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastGenerated { get; set; }
        public int Id { get; set; }

        public virtual Corpus SourceCorpus { get; set; }
        public virtual Corpus TargetCorpus { get; set; }
        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }
    }
}
