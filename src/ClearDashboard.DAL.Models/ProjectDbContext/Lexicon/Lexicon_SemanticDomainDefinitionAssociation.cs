
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomainDefinitionAssociation : IdentifiableEntity
    {
        public Lexicon_SemanticDomainDefinitionAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey(nameof(SemanticDomainId))]
        public Guid SemanticDomainId { get; set; }
        public virtual Lexicon_SemanticDomain? SemanticDomain { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public Guid DefinitionId { get; set; }
        public virtual Lexicon_Definition? Definition { get; set; }
    }
}
