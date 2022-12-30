
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Meaning : SynchronizableTimestampedEntity
    {
        public Lexicon_Meaning()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Translations = new HashSet<Lexicon_Translation>();
            SemanticDomains = new HashSet<Lexicon_SemanticDomain>();
            SemanticDomainMeaningAssociations = new HashSet<Lexicon_SemanticDomainMeaningAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? Text { get; set; }

        [ForeignKey(nameof(LexemeId))]
        public Guid LexemeId { get; set; }
        public virtual Lexicon_Lexeme? Lexeme { get; set; }
        public ICollection<Lexicon_Translation> Translations { get; set; }
        public ICollection<Lexicon_SemanticDomain> SemanticDomains { get; set; }
        public ICollection<Lexicon_SemanticDomainMeaningAssociation> SemanticDomainMeaningAssociations { get; set; }
    }
}
