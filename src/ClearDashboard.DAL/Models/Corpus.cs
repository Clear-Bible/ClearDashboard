using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Corpus
    {
        public Corpus()
        {
            Verses = new HashSet<Verse>();
        }

        public long Id { get; set; }
        public byte[] IsRtl { get; set; }
        public string Name { get; set; }
        public long? Language { get; set; }
        public string ParatextGuid { get; set; }
        public long? CorpusTypeId { get; set; }

        public virtual ParallelCorpus ParallelCorpus { get; set; }
        public virtual ParallelCorpus CorpusNavigation { get; set; }
        public virtual CorpusType CorpusType { get; set; }
        public virtual ICollection<Verse> Verses { get; set; }
    }
}
