using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedTokenizedCorpus
{
    public string ParatextProjectId = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public CorpusType CorpusType = CorpusType.Standard;
    public List<SerializedTokenization> Tokenizations { get;set;} = new();
    public Guid CorpusId { get; set; } = Guid.Empty;
    public bool IsRTL { get; set; }
    public string TranslationFontFamily { get; set; } = "Segoe UI";
}