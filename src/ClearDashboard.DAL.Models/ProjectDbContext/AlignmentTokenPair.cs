using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentTokenPair : SynchronizableTimestampedEntity
{
    public Guid SourceTokenId { get; set; }
    public Guid TargetTokenId { get; set; }
      
    public virtual AlignmentType AlignmentType { get; set; }
    public virtual AlignmentState AlignmentState { get; set; }

    public virtual double Probability { get; set; }

    [ForeignKey("SourceTokenId")]
    public virtual Token? SourceToken { get; set; }
    [ForeignKey("TargetTokenId")]
    public virtual Token? TargetToken { get; set; }
}