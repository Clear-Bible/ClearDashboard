namespace ClearDashboard.DataAccessLayer.Models;

public class CorpusVersion : SynchronizableTimestampedEntity
{
    public bool IsRtl { get; set; }
    public string? Name { get; set; }
    public string? Language { get; set; }
    public string? ParatextGuid { get; set; }
    public virtual CorpusType CorpusType { get; set; }

    public virtual Guid CorpusId { get; set; }
    public virtual Corpus? Corpus { get; set; }

       
}