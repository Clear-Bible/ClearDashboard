using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(InterlinearEnhancedViewItemMetadatum))]
public class InterlinearEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? TranslationSetId { get; set; }
}