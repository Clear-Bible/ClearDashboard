using AvalonDock.Layout;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(InterlinearEnhancedViewItemMetadatum))]
public class InterlinearEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? TranslationSetId { get; set; }

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {
            ContentId = TranslationSetId,
            Content = viewModel,
            Title = DisplayName,
            IsActive = true
        };
    }
}