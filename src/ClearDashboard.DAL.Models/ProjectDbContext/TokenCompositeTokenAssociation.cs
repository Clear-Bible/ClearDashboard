using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class TokenCompositeTokenAssociation : IdentifiableEntity
    {
        [ForeignKey(nameof(TokenId))]
        public Guid TokenId { get; set; }
        public Token? Token { get; set; }

        [ForeignKey(nameof(TokenCompositeId))]
        public Guid TokenCompositeId { get; set; }
        public TokenComposite? TokenComposite { get; set; }

        public DateTimeOffset? Deleted { get; set; }
    }
}
