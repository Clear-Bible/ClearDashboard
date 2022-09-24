using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Alignment : IdentifiableEntity
{
    public Guid SourceTokenComponentId { get; set; }
    public Guid TargetTokenComponentId { get; set; }
      
    public virtual AlignmentVerification AlignmentVerification { get; set; }
    public virtual AlignmentOriginatedFrom AlignmentOriginatedFrom { get; set; }

    public virtual double Score { get; set; }

    [ForeignKey("SourceTokenId")]
    public virtual TokenComponent? SourceTokenComponent { get; set; }
    [ForeignKey("TargetTokenId")]
    public virtual TokenComponent? TargetTokenComponent { get; set; }

    [ForeignKey("AlignmentSetId")]
    public Guid AlignmentSetId { get; set; }
    public virtual AlignmentSet? AlignmentSet { get; set; }
}