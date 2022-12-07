
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LexicalItemDefinition : SynchronizableTimestampedEntity
    {
        public LexicalItemDefinition()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            SemanticDomains = new HashSet<SemanticDomain>();
            SemanticDomainLexicalItemDefinitionAssociations = new HashSet<SemanticDomainLexicalItemDefinitionAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? TrainingText { get; set; }

        [ForeignKey(nameof(LexicalItemId))]
        public Guid LexicalItemId { get; set; }
        public virtual LexicalItem? LexicalItem { get; set; }
        public ICollection<SemanticDomain> SemanticDomains { get; set; }
        public ICollection<SemanticDomainLexicalItemDefinitionAssociation> SemanticDomainLexicalItemDefinitionAssociations { get; set; }
    }
}
