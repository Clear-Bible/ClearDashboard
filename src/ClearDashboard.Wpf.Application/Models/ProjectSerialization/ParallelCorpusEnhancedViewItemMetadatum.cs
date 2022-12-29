
using System;
using System.Text.Json.Serialization;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public abstract class ParallelCorpusEnhancedViewItemMetadatum : VerseAwareEnhancedViewItemMetadatum
{
    public string? DisplayName { get; set; }
    public string? ParallelCorpusId { get; set; }
    public string? ParallelCorpusDisplayName { get; set; }
    public bool? IsRtl { get; set; }
    public bool? IsTargetRtl { get; set; }
    public string? SourceParatextId { get; set; }
    public string? TargetParatextId { get; set; }

    [JsonIgnore]
    public ParallelCorpus? ParallelCorpus { get; set; }
}