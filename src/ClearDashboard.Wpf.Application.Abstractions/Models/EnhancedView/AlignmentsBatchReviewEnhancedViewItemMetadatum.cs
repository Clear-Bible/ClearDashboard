using System;
using System.Text.Json.Serialization;
using AvalonDock.Layout;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using Dahomey.Json.Attributes;


namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(AlignmentsBatchReviewEnhancedViewItemMetadatum))]
public class AlignmentsBatchReviewEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{

    public string? AlignmentSetId { get; set; }

    public string? ParallelCorpusId { get; set; }
    public string? ParallelCorpusDisplayName { get; set; }
    public bool? IsRtl { get; set; }
    public bool? IsTargetRtl { get; set; }
    public string? SourceParatextId { get; set; }
    public string? TargetParatextId { get; set; }

    [JsonIgnore]
    public ParallelCorpus? ParallelCorpus { get; set; }


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