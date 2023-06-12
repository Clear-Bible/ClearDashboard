using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Translation : SynchronizableTimestampedEntity
{
    public Guid SourceTokenComponentId { get; set; }
    public string? TargetText { get; set; }
    public virtual TranslationOriginatedFrom TranslationState { get; set; }

    [ForeignKey(nameof(TranslationSetId))]
    public Guid TranslationSetId { get; set; }
    public virtual TranslationSet? TranslationSet { get; set; }

    [ForeignKey(nameof(SourceTokenComponentId))]
    public virtual TokenComponent? SourceTokenComponent { get; set; }

    [ForeignKey(nameof(LexiconTranslationId))]
    public Guid? LexiconTranslationId { get; set; }
    public virtual Lexicon_Translation? LexiconTranslation { get; set; }

    public DateTimeOffset? Modified { get; set; }
    public DateTimeOffset? Deleted { get; set; }
}