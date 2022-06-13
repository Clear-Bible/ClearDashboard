using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class ParallelCorpusVersion : IdentifiableEntity
{
    public ParallelCorpusVersion()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        //ParallelVersesLinks = new HashSet<ParallelVersesLink>();
        // ReSharper restore VirtualMemberCallInConstructor
    }

    public Guid SourceCorpusId { get; set; }
    public Guid TargetCorpusId { get; set; }

    public AlignmentType AlignmentType { get; set; }
  
    public DateTimeOffset LastGenerated { get; set; }

    [ForeignKey("SourceCorpusId")]
    public virtual Corpus? SourceCorpus { get; set; }
    [ForeignKey("TargetCorpusId")]
    public virtual Corpus? TargetCorpus { get; set; }
    // public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }
}