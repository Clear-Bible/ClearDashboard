
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Adornment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? TokenId { get; set; }
        public string? Lemma { get; set; }
        public string? PartsOfSpeech { get; set; }
        public string? Strong { get; set; }

        public virtual Token? Token { get; set; }
    }
}
