using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentSetDenormalizationTask : IdentifiableEntity
{
    public AlignmentSetDenormalizationTask()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        // ReSharper restore VirtualMemberCallInConstructor
    }

    [ForeignKey("AlignmentSetId")]
    public virtual Guid AlignmentSetId { get; set; }
    public virtual AlignmentSet? AlignmentSet { get; set; }
    public string? SourceText { get; set; }
}