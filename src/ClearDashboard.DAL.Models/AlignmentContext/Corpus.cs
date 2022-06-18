
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Corpus : SynchronizableTimestampedEntity
    {
        public Corpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenizedCorpora = new HashSet<TokenizedCorpus>();
            Verses = new HashSet<Verse>();
            SourceParallelCorpusVersions = new HashSet<ParallelCorpusVersion>();
            TargetParallelCorpusVersions = new HashSet<ParallelCorpusVersion>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public virtual ICollection<Verse> Verses { get; set; }
        public virtual ICollection<CorpusVersion> Versions { get; set; }
        public virtual ICollection<TokenizedCorpus> TokenizedCorpora { get; set; }

        public virtual ICollection<ParallelCorpusVersion> SourceParallelCorpusVersions { get; set; }
        public virtual ICollection<ParallelCorpusVersion> TargetParallelCorpusVersions { get; set; }


    }
}
