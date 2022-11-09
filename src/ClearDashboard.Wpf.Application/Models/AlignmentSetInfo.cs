using System.Drawing;

namespace ClearDashboard.Wpf.Application.Models;

public class AlignmentSetInfo
{
    public string AlignmentSetId { get; set; } = string.Empty;
    public string? DisplayName { get; set; } = string.Empty;

    public string ParallelCorpusId { get; set; } = string.Empty;
    public string? ParallelCorpusDisplayName { get; set; } = string.Empty;

    public bool IsRtl { get; set; } = false;

    public bool IsTargetRtl { get; set; } = false;

    public string SourceFontFamily { get; set; } = "Segoe UI";
    public string TargetFontFamily { get; set; } = "Segoe UI";
}