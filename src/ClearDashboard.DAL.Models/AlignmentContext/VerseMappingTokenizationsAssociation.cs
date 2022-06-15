namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMappingTokenizationsAssociation : SynchronizableEntity
{
    public Guid? SourceTokenizationId { get; set; }
    public Tokenization? SourceTokenization { get; set; }

    public Guid? TargetTokenizationId { get; set; }
    public Tokenization? TargetTokenization { get; set; }

    public Guid? VerseMappingId { get; set; }
    public virtual VerseMapping? VerseMapping { get; set; }

    //public Guid? TokenizationId { get; set; }
    //public Tokenization? Tokenization { get; set; }
}