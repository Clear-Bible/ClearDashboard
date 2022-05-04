using System.Collections.ObjectModel;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelVersesLink
    {
        public int Id { get; set; }
       
        public int? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus ParallelCorpus { get; set; }
        public virtual Collection<VerseLink> VerseLinks { get; set; }
    }
}
