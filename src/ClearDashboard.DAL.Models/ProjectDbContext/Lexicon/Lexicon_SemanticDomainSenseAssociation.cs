
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomainSenseAssociation : IdentifiableEntity
    {
        public Lexicon_SemanticDomainSenseAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey(nameof(SemanticDomainId))]
        public Guid SemanticDomainId { get; set; }
        public virtual Lexicon_SemanticDomain? SemanticDomain { get; set; }

        [ForeignKey(nameof(SenseId))]
        public Guid SenseId { get; set; }
        public virtual Lexicon_Sense? Sense { get; set; }
    }
}
