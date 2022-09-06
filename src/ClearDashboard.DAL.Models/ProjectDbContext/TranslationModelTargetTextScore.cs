using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class TranslationModelTargetTextScore : IdentifiableEntity
{
    [ForeignKey("TranslationModelEntryId")]
    public Guid? TranslationModelEntryId { get; set; }
    public virtual TranslationModelEntry? TranslationModelEntry { get; set; }
    public string? Text { get; set; }
    public double Score { get; set; }
}