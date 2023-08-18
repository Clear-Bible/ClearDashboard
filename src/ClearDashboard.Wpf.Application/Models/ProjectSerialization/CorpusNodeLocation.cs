using System;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class CorpusNodeLocation
{

    public double X { get; set; }
    public double Y { get; set; }
    public Guid CorpusId { get; set; } = Guid.Empty;

    public string? CorpusName { get; set; } = string.Empty;
}
