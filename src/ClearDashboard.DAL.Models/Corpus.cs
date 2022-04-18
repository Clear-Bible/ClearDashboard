
using ClearDashboard.Common.Models;

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
        public virtual CorpusType CorpusType { get; set; }
        public virtual ICollection<Verse> Verses { get; set; }
    }
}
