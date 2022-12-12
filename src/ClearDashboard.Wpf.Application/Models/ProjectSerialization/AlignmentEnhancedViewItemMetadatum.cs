using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

[JsonDiscriminator(nameof(AlignmentEnhancedViewItemMetadatum))]
public class AlignmentEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? AlignmentSetId { get; set; }
}