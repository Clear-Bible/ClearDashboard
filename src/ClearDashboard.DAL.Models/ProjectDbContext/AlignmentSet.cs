using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentSet : SynchronizableTimestampedEntity
{
    public AlignmentSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Alignments = new HashSet<Alignment>();
        Metadata = new Dictionary<string, object>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    //[ForeignKey("EngineWordAlignmentId")]
    //public Guid? EngineWordAlignmentId { get; set; }
    //public virtual EngineWordAlignment? EngineWordAlignment { get; set; }
    [ForeignKey("ParallelCorpusId")]
    public virtual Guid ParallelCorpusId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }
    public string? DisplayName { get; set; }
    public string? SmtModel { get; set; }
    public bool IsSyntaxTreeAlignerRefined { get; set; }
    public bool IsSymmetrized { get; set; }

    public virtual ICollection<Alignment> Alignments { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; }
}