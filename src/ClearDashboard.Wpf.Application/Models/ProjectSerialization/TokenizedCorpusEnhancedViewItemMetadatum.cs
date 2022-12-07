using System;
using ClearDashboard.DataAccessLayer.Models;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

[JsonDiscriminator(nameof(TokenizedCorpusEnhancedViewItemMetadatum))]
public class TokenizedCorpusEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{
    public string? ParatextProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? TokenizationType { get; set; }
    public Guid? CorpusId { get; set; }
    public Guid? TokenizedTextCorpusId { get; set; }
    public CorpusType CorpusType { get; set; }
}