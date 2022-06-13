
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Corpus : IdentifiableEntity
    {
        public Corpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Tokenizations = new HashSet<Tokenization>();
            //Versions = new HashSet<CorpusVersion>();
            Verses = new HashSet<Verse>();
            SourceParallelCorpusVersions = new HashSet<ParallelCorpusVersion>();
            TargetParallelCorpusVersions = new HashSet<ParallelCorpusVersion>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public virtual ICollection<Verse> Verses { get; set; }
        public virtual ICollection<CorpusVersion> Versions { get; set; }
        //public virtual ICollection<CorpusVersion> Versions { get; set; }
        public virtual ICollection<Tokenization> Tokenizations { get; set; }

        public virtual ICollection<ParallelCorpusVersion> SourceParallelCorpusVersions { get; set; }
        public virtual ICollection<ParallelCorpusVersion> TargetParallelCorpusVersions { get; set; }


    }
}
