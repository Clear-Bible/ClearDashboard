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

        public int Id { get; set; }
        public bool IsRtl { get; set; }
        public string Name { get; set; }
        public int? Language { get; set; }
        public string ParatextGuid { get; set; }
        public int? CorpusTypeId { get; set; }

        public virtual ParallelCorpus ParallelCorpus { get; set; }
        public virtual ParallelCorpus CorpusNavigation { get; set; }
        public virtual CorpusType CorpusType { get; set; }
        public virtual ICollection<Verse> Verses { get; set; }
    }
}
