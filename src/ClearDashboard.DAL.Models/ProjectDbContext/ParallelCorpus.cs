using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParallelCorpus : SynchronizableTimestampedEntity
    {
        public ParallelCorpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            VerseMappings = new HashSet<VerseMapping>();
            //ParallelTokenizedCopora = new HashSet<ParallelTokenizedCorpus>();
            AlignmentSets = new HashSet<AlignmentSet>();
            TranslationSets = new HashSet<TranslationSet>();
            // ReSharper restore VirtualMemberCallInConstructor
        }


        public AlignmentType AlignmentType { get; set; }

        public DateTimeOffset LastGenerated { get; set; }

        public Guid SourceTokenizedCorpusId { get; set; }
        [ForeignKey(nameof(SourceTokenizedCorpusId))]
        public virtual TokenizedCorpus? SourceTokenizedCorpus { get; set; }

        public Guid TargetTokenizedCorpusId { get; set; }
        [ForeignKey(nameof(TargetTokenizedCorpusId))]
        public virtual TokenizedCorpus? TargetTokenizedCorpus { get; set; }

        public virtual ICollection<VerseMapping> VerseMappings { get; set; }
        public virtual ICollection<AlignmentSet> AlignmentSets { get; set; }
        public virtual ICollection<TranslationSet> TranslationSets { get; set; }
    }
}
