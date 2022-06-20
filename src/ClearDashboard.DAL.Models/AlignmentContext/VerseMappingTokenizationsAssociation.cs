namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMappingTokenizationsAssociation : SynchronizableTimestampedEntity
{
    public Guid? SourceTokenizationId { get; set; }
    public Tokenization? SourceTokenization { get; set; }

    public Guid? TargetTokenizationId { get; set; }
    public Tokenization? TargetTokenization { get; set; }

    public Guid? VerseMappingId { get; set; }
    public virtual VerseMapping? VerseMapping { get; set; }
}