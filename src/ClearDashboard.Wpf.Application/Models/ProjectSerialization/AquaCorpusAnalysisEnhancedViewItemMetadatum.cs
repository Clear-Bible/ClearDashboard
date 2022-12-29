using System;
using System.Text.Json.Serialization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using Dahomey.Json.Attributes;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

[JsonDiscriminator(nameof(AquaCorpusAnalysisEnhancedViewItemMetadatum))]
public class AquaCorpusAnalysisEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
{
    public string? DisplayName { get; set; }
    public string? ParatextProjectId { get; set; }
}