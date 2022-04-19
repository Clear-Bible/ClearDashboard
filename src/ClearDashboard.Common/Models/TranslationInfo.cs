namespace ClearDashboard.Common.Models;

public class TranslationInfo
{
    public CorpusType CorpusType { get; set; } = CorpusType.Unknown;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectGuid { get; set; } = string.Empty;
}