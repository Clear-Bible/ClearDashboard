using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(AlignmentEnhancedViewItemMetadatum))]
public class AlignmentEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? AlignmentSetId { get; set; }
}