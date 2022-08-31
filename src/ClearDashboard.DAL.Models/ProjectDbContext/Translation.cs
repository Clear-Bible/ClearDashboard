using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class Translation : SynchronizableTimestampedEntity
{
    public Guid TokenId { get; set; }
    public string? TargetText { get; set; }
    public virtual TranslationState TranslationState { get; set; }

    [ForeignKey("TokenId")]
    public virtual Token? Token { get; set; }
}