using System;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public class AlignmentGroup : ModelGroup<Models.Alignment>
{
    public Guid AlignmentSetId { get; set; }
    public TokenizedCorpusExtra SourceTokenizedCorpus { get; set; } = new();
    public TokenizedCorpusExtra TargetTokenizedCorpus { get; set; } = new();
    public string Location { get; set; } = string.Empty;
}

