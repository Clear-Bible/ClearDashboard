using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Aqua.Module.Models;

[JsonDiscriminator(nameof(AquaCorpusAnalysisEnhancedViewItemMetadatum))]
public class AquaCorpusAnalysisEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{
    public string? UrlString { get; set; }

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {
            ContentId = UrlString,
            Content = viewModel,
            Title = $"⳼ {viewModel.Title}",
            IsActive = true
        };
    }
}