using System;
using AvalonDock.Layout;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Converters;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(AlignmentEnhancedViewItemMetadatum))]
public class AlignmentEnhancedViewItemMetadatum : ParallelCorpusEnhancedViewItemMetadatum
{
    public string? AlignmentSetId { get; set; }

    public EditMode EditMode { get; set; } = EditMode.MainViewOnly;

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {
            ContentId = AlignmentSetId,
            Content = viewModel,
            Title = DisplayName,
            IsActive = true
        };
    }

  
}