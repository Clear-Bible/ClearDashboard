using System;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public class TokenizedTokenGroup : ModelGroup<Models.Token>
{
    public Guid TokenizedCorpusId { get; set; } = Guid.Empty;
    public string BookLocation { get; set; } = string.Empty;
}

