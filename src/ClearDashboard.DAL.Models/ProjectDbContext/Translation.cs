using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Translation : SynchronizableTimestampedEntity
{
    public Guid SourceTokenId { get; set; }
    public string? TargetText { get; set; }
    public virtual TranslationOriginatedFrom TranslationState { get; set; }

    [ForeignKey("TranslationSetId")]
    public Guid TranslationSetId { get; set; }
    public virtual TranslationSet? TranslationSet { get; set; }

    [ForeignKey("SourceTokenId")]
    public virtual Token? SourceToken { get; set; }
}