using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Alignment : SynchronizableTimestampedEntity
{
    public Guid SourceTokenId { get; set; }
    public Guid TargetTokenId { get; set; }
      
    public virtual AlignmentVerification AlignmentVerification { get; set; }
    public virtual AlignmentOriginatedFrom AlignmentOriginatedFrom { get; set; }

    public virtual double Score { get; set; }

    [ForeignKey("SourceTokenId")]
    public virtual Token? SourceToken { get; set; }
    [ForeignKey("TargetTokenId")]
    public virtual Token? TargetToken { get; set; }
}