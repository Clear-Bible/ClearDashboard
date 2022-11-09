using System;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedCorpus
{
    public string? CorpusId { get; set; }
    public bool IsRtl { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Language { get; set; }
    public string? ParatextGuid { get; set; }
    public string? CorpusType { get; set; }
    public DateTimeOffset? Created { get; set; }
    public string? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string TranslationFontFamily { get; set; } = "Segoe UI";
}