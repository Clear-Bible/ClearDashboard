using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Aqua.Module.Models;

[JsonDiscriminator(nameof(AquaCorpusAnalysisEnhancedViewItemMetadatum))]
public class AquaCorpusAnalysisEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{
    public string? AssessmentId { get; set; }

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {
            ContentId = AssessmentId,
            Content = viewModel,
            Title = $"⳼ {viewModel.Title}",
            IsActive = true
        };
    }
}