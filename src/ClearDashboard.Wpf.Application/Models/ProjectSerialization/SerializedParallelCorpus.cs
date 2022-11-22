using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.Generic;
using System.Drawing;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedParallelCorpus
{
    public string SourceConnectorId { get; set; } = string.Empty;
    public string SourceFontFamily { get; set; } = "Segoe UI";
    public string TargetConnectorId { get; set; } = string.Empty;
    public string TargetFontFamily { get; set; } = "Segoe UI";
    public string ParallelCorpusId { get; set; } = string.Empty;
    public string? ParallelCorpusDisplayName { get; set; } = string.Empty;
    public List<TranslationSetInfo> TranslationSetInfo { get; set; } = new();
    public List<AlignmentSetInfo> AlignmentSetInfo { get; set; } = new();
}
