
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Corpus : SynchronizableTimestampedEntity
    {
        public Corpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenizedCorpora = new HashSet<TokenizedCorpus>();
            Verses = new HashSet<Verse>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public bool IsRtl { get; set; }
        public string? Name { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public virtual CorpusType CorpusType { get; set; }

        public virtual ICollection<Verse> Verses { get; set; }
        public virtual ICollection<TokenizedCorpus> TokenizedCorpora { get; set; }

    }
}
