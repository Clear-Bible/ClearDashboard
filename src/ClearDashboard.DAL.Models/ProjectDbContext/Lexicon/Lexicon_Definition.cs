
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Definition : SynchronizableTimestampedEntity
    {
        public Lexicon_Definition()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Translations = new HashSet<Lexicon_Translation>();
            SemanticDomains = new HashSet<Lexicon_SemanticDomain>();
            SemanticDomainDefinitionAssociations = new HashSet<Lexicon_SemanticDomainDefinitionAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? Text { get; set; }

        [ForeignKey(nameof(LexicalItemId))]
        public Guid LexicalItemId { get; set; }
        public virtual Lexicon_LexicalItem? LexicalItem { get; set; }
        public ICollection<Lexicon_Translation> Translations { get; set; }
        public ICollection<Lexicon_SemanticDomain> SemanticDomains { get; set; }
        public ICollection<Lexicon_SemanticDomainDefinitionAssociation> SemanticDomainDefinitionAssociations { get; set; }
    }
}
