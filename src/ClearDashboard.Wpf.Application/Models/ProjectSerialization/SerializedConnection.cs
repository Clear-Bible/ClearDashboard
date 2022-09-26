using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedConnection
{
    public string SourceConnectorId { get; set; } = string.Empty;
    public string TargetConnectorId { get; set; } = string.Empty;
    public List<TranslationSetInfo> TranslationSetInfo { get; set; } = new();
}
