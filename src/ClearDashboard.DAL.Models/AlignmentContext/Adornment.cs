
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Adornment : SynchronizableTimestampedEntity
    {
       
        public Guid? TokenId { get; set; }
        public string? Lemma { get; set; }
        public string? PartsOfSpeech { get; set; }
        public string? Strong { get; set; }

        public virtual Token? Token { get; set; }
    }
}
