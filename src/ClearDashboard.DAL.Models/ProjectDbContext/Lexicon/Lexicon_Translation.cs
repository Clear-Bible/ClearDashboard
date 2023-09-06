
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Translation : SynchronizableTimestampedEntity
    {
        public Lexicon_Translation()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }

        [Column(TypeName = "jsonb")]
        public string? OriginatedFrom { get; set; }

        [ForeignKey(nameof(MeaningId))]
        public Guid MeaningId { get; set; }
        public virtual Lexicon_Meaning? Meaning { get; set; }
    }
}
