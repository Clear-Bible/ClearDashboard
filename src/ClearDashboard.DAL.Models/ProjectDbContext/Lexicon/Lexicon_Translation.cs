
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

        [ForeignKey(nameof(SenseId))]
        public Guid SenseId { get; set; }
        public virtual Lexicon_Sense? Sense { get; set; }
    }
}
