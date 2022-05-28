using System.Collections.ObjectModel;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelVersesLink
    {
        public ParallelVersesLink()
        {
            VerseLinks = new HashSet<VerseLink>();
        }
        public int Id { get; set; }
       
        public int? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus? ParallelCorpus { get; set; }
        public virtual ICollection<VerseLink> VerseLinks { get; set; }
    }
}
