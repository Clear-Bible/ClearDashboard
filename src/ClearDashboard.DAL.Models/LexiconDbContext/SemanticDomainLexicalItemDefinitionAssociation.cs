
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class SemanticDomainLexicalItemDefinitionAssociation : IdentifiableEntity
    {
        public SemanticDomainLexicalItemDefinitionAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey(nameof(SemanticDomainId))]
        public Guid SemanticDomainId { get; set; }
        public virtual SemanticDomain? SemanticDomain { get; set; }

        [ForeignKey(nameof(LexicalItemDefinitionId))]
        public Guid LexicalItemDefinitionId { get; set; }
        public virtual LexicalItemDefinition? LexicalItemDefinition { get; set; }
    }
}
