
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomain : SynchronizableTimestampedEntity
    {
        public Lexicon_SemanticDomain()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Definitions = new HashSet<Lexicon_Definition>();
            SemanticDomainDefinitionAssociations = new HashSet<Lexicon_SemanticDomainDefinitionAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }
        public ICollection<Lexicon_Definition> Definitions { get; set; }
        public ICollection<Lexicon_SemanticDomainDefinitionAssociation> SemanticDomainDefinitionAssociations { get; set; }
    }
}
