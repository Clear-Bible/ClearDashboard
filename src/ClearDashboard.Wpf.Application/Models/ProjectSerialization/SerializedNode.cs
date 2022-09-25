using System;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class SerializedNode
{
    public string ParatextProjectId = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public CorpusType CorpusType = CorpusType.Standard;
    public List<SerializedTokenization> NodeTokenizations = new();
    public Guid CorpusId { get; set; } = Guid.Empty;
}