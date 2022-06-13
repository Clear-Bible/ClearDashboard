namespace ClearDashboard.DataAccessLayer.Models;

public class CorpusVersion : IdentifiableEntity
{
    public bool IsRtl { get; set; }
    public string? Name { get; set; }
    public int? Language { get; set; }
    public string? ParatextGuid { get; set; }
    public virtual CorpusType CorpusType { get; set; }
       
}