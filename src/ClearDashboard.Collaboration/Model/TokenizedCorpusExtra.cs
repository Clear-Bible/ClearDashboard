using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public class TokenizedCorpusExtra : ModelExtra
{
    public Guid Id { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Tokenization { get; set; } = string.Empty;
    public DateTimeOffset? LastTokenized { get; set; } = null;
}

public abstract class ModelExtra
{
}

