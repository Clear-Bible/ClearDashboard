using System;
using System.Text.Json.Serialization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
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
}