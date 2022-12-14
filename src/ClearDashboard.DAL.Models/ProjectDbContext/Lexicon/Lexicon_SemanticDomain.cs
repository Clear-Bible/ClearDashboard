
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomain : SynchronizableTimestampedEntity
    {
        public Lexicon_SemanticDomain()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Senses = new HashSet<Lexicon_Sense>();
            SemanticDomainSenseAssociations = new HashSet<Lexicon_SemanticDomainSenseAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }
        public ICollection<Lexicon_Sense> Senses { get; set; }
        public ICollection<Lexicon_SemanticDomainSenseAssociation> SemanticDomainSenseAssociations { get; set; }
    }
}
