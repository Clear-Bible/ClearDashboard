
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_SemanticDomain : SynchronizableTimestampedEntity
    {
        public Lexicon_SemanticDomain()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Meanings = new HashSet<Lexicon_Meaning>();
            SemanticDomainMeaningAssociations = new HashSet<Lexicon_SemanticDomainMeaningAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }
        public ICollection<Lexicon_Meaning> Meanings { get; set; }
        public ICollection<Lexicon_SemanticDomainMeaningAssociation> SemanticDomainMeaningAssociations { get; set; }
    }
}
