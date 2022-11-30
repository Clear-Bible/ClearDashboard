
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class SemanticDomain : IdentifiableEntity
    {
        public SemanticDomain()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            LexicalItemDefinitions = new HashSet<LexicalItemDefinition>();
            SemanticDomainLexicalItemDefinitionAssociations = new HashSet<SemanticDomainLexicalItemDefinitionAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }
        public ICollection<LexicalItemDefinition> LexicalItemDefinitions { get; set; }
        public ICollection<SemanticDomainLexicalItemDefinitionAssociation> SemanticDomainLexicalItemDefinitionAssociations { get; set; }
    }
}
