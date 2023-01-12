using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(AquaCorpusAnalysisEnhancedViewItemMetadatum))]
public class AquaCorpusAnalysisEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{
    public string? DisplayName { get; set; }
    public string? ParatextProjectId { get; set; }
}