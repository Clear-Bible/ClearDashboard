using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class CorpusType
    {
        public CorpusType()
        {
            Corpa = new HashSet<Corpus>();
        }

        public long Id { get; set; }
        public long? Description { get; set; }

        public virtual ICollection<Corpus> Corpa { get; set; }
    }
}
