using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class TranslationModelEntry : IdentifiableEntity
{
    public TranslationModelEntry()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        TargetTextScores = new HashSet<TranslationModelTargetTextScore>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    [ForeignKey(nameof(TranslationSetId))]
    public Guid TranslationSetId { get; set; }
    public virtual TranslationSet? TranslationSet { get; set; }
    public string? SourceText { get; set; }

    public virtual ICollection<TranslationModelTargetTextScore> TargetTextScores { get; set; }
}