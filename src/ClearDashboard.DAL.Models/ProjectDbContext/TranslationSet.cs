using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class TranslationSet : IdentifiableEntity
{
    public TranslationSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        TranslationModel = new HashSet<TranslationModelEntry>();
        Translations = new HashSet<Translation>();
        var utcNow = DateTimeOffset.UtcNow;
        Created = utcNow;
        // ReSharper restore VirtualMemberCallInConstructor
    }
    [ForeignKey("EngineWordAlignmentId")]
    public Guid? EngineWordAlignmentId { get; set; }
    public virtual EngineWordAlignment? EngineWordAlignment { get; set; }

    public virtual TranslationSet? DerivedFrom { get; set; }

    public virtual Guid ParallelCorpusId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }

    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    public virtual ICollection<TranslationModelEntry> TranslationModel { get; set; }
    public virtual ICollection<Translation> Translations { get; set; }
}