using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(AlignmentsBatchReviewEnhancedViewItemMetadatum))]
public class AlignmentsBatchReviewEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{

    public string? AlignmentSetId { get; set; }

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {

            // TODO:  change
            ContentId = AlignmentSetId,
            Content = viewModel,
            Title = DisplayName,
            IsActive = true
        };
    }
}