using System;
using System.Text.Json.Serialization;
using AvalonDock.Layout;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

[JsonDiscriminator(nameof(TokenizedCorpusEnhancedViewItemMetadatum))]
public class TokenizedCorpusEnhancedViewItemMetadatum : VerseAwareEnhancedViewItemMetadatum
{
    public string? ParatextProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? TokenizationType { get; set; }
    public Guid? CorpusId { get; set; }
    public Guid? TokenizedTextCorpusId { get; set; }
    public CorpusType CorpusType { get; set; }

    public bool? IsRtl { get; set; }

    [JsonIgnore]
    public TokenizedTextCorpus? TokenizedTextCorpus { get; set; }

    public override LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel)
    {
        return new LayoutDocument
        {
            ContentId = ParatextProjectId,
            Content = viewModel,
            Title = $"⳼ {ProjectName} ({TokenizationType})",
            IsActive = true
        };
    }
}