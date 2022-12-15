using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

[JsonDiscriminator(nameof(InterlinearEnhancedViewItemMetadatum))]
public class InterlinearEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? TranslationSetId { get; set; }
}