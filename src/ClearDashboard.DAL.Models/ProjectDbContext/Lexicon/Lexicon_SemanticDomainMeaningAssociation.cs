
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomainMeaningAssociation : IdentifiableEntity
    {
        public Lexicon_SemanticDomainMeaningAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey(nameof(SemanticDomainId))]
        public Guid SemanticDomainId { get; set; }
        public virtual Lexicon_SemanticDomain? SemanticDomain { get; set; }

        [ForeignKey(nameof(MeaningId))]
        public Guid MeaningId { get; set; }
        public virtual Lexicon_Meaning? Meaning { get; set; }
    }
}
