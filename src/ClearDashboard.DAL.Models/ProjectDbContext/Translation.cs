using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Translation : IdentifiableEntity
{
    public Guid SourceTokenComponentId { get; set; }
    public string? TargetText { get; set; }
    public virtual TranslationOriginatedFrom TranslationState { get; set; }

    [ForeignKey("TranslationSetId")]
    public Guid TranslationSetId { get; set; }
    public virtual TranslationSet? TranslationSet { get; set; }

    [ForeignKey("SourceTokenComponentId")]
    public virtual TokenComponent? SourceTokenComponent { get; set; }
}