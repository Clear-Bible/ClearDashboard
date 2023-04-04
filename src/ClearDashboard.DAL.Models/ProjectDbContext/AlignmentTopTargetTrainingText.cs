using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentTopTargetTrainingText
{
    public AlignmentTopTargetTrainingText()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        SourceTrainingText = string.Empty;
        TopTargetTrainingText = string.Empty;
        // ReSharper restore VirtualMemberCallInConstructor
    }

    public Guid Id { get; set; }

    [ForeignKey("AlignmentSetId")]
    public Guid AlignmentSetId { get; set; }
    public virtual AlignmentSet? AlignmentSet { get; set; }

    [ForeignKey("SourceTokenComponentId")]
    public Guid SourceTokenComponentId { get; set; }
    public virtual TokenComponent? SourceTokenComponent { get; set; }

    public string SourceTrainingText { get; set; }
    public string TopTargetTrainingText { get; set; }
}