using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class ParallelCorpusVersion : SynchronizableTimestampedEntity
{
    public ParallelCorpusVersion()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        VerseMappings = new HashSet<VerseMapping>();
        ParallelTokenizedCopora = new HashSet<ParallelTokenizedCorpus>();
        // ReSharper restore VirtualMemberCallInConstructor
    }


    public AlignmentType AlignmentType { get; set; }

    public DateTimeOffset LastGenerated { get; set; }

    public Guid SourceCorpusId { get; set; }
    [ForeignKey(nameof(SourceCorpusId))]
    public virtual Corpus? SourceCorpus { get; set; }

    public Guid TargetCorpusId { get; set; }
    [ForeignKey(nameof(TargetCorpusId))]
    public virtual Corpus? TargetCorpus { get; set; }
    public virtual ICollection<VerseMapping> VerseMappings { get; set; }
    public virtual ICollection<ParallelTokenizedCorpus> ParallelTokenizedCopora { get; set; }
}