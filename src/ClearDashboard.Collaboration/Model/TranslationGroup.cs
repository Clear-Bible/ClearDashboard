using System;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public class TranslationGroup : ModelGroup<Models.Translation>
{
    public Guid TranslationSetId { get; set; }
    public TokenizedCorpusExtra SourceTokenizedCorpus { get; set; } = new();
    public string Location { get; set; } = string.Empty;
}