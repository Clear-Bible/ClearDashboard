using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class TranslationSet :  SynchronizableTimestampedEntity
{
    public TranslationSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        TranslationModel = new HashSet<TranslationModelEntry>();
        Translations = new HashSet<Translation>();
        Metadata = new Dictionary<string, object>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    //[ForeignKey("EngineWordAlignmentId")]
    //public Guid? EngineWordAlignmentId { get; set; }
    //public virtual EngineWordAlignment? EngineWordAlignment { get; set; }

    public virtual TranslationSet? DerivedFrom { get; set; }

    [ForeignKey("ParallelCorpusId")]
    public virtual Guid ParallelCorpusId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }

    [ForeignKey("AlignmentSetId")]
    public virtual Guid AlignmentSetId { get; set; }
    public virtual AlignmentSet? AlignmentSet { get; set; }

    public string? DisplayName { get; set; }
    //public string? SmtModel { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; }

    public virtual ICollection<TranslationModelEntry> TranslationModel { get; set; }
    public virtual ICollection<Translation> Translations { get; set; }
}