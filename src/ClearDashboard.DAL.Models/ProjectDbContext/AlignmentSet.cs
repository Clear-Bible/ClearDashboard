using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentSet : SynchronizableTimestampedEntity
{
    public AlignmentSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        AlignmentTokensPairs = new HashSet<AlignmentTokenPair>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    [ForeignKey("EngineWordAlignmentId")]
    public Guid? EngineWordAlignmentId { get; set; }
    public virtual EngineWordAlignment? EngineWordAlignment { get; set; }
    [ForeignKey("ParallelCorpusId")]
    public virtual Guid ParallelCorpusId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }
    public virtual User? User { get; set; }

    public virtual ICollection<AlignmentTokenPair> AlignmentTokensPairs { get; set; }
}