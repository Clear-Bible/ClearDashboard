using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.Generic;
using System.Drawing;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedConnection
{
    public string SourceConnectorId { get; set; } = string.Empty;
    public FontFamily SourceFontFamily { get; set; } = new FontFamily("Segoe UI");
    public string TargetConnectorId { get; set; } = string.Empty;
    public FontFamily TargetFontFamily { get; set; } = new FontFamily("Segoe UI");
    public string ParallelCorpusId { get; set; } = string.Empty;
    public string? ParallelCorpusDisplayName { get; set; } = string.Empty;
    public List<TranslationSetInfo> TranslationSetInfo { get; set; } = new();
    public List<AlignmentSetInfo> AlignmentSetInfo { get; set; } = new();
}
