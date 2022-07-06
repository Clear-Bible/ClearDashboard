
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Adornment : SynchronizableTimestampedEntity
    {
        public string? Lemma { get; set; }
        public string? PartsOfSpeech { get; set; }
        public string? Strong { get; set; }

        // CODE REVIEW:  Need some examples
        public string? TokenMorphology { get; set; }

        public Guid? TokenId { get; set; }
        public virtual Token? Token { get; set; }
    }
}
