using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Alignment : SynchronizableTimestampedEntity
{
    public Guid SourceTokenComponentId { get; set; }
    public Guid TargetTokenComponentId { get; set; }
      
    public virtual AlignmentVerification AlignmentVerification { get; set; }
    public virtual AlignmentOriginatedFrom AlignmentOriginatedFrom { get; set; }

    public virtual double Score { get; set; }

    [ForeignKey("SourceTokenComponentId")]
    public virtual TokenComponent? SourceTokenComponent { get; set; }
    [ForeignKey("TargetTokenComponentId")]
    public virtual TokenComponent? TargetTokenComponent { get; set; }

    [ForeignKey("AlignmentSetId")]
    public Guid AlignmentSetId { get; set; }
    public virtual AlignmentSet? AlignmentSet { get; set; }

    public DateTimeOffset? Deleted { get; set; }
}