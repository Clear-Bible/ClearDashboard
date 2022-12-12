using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class TokenVerseAssociation : SynchronizableTimestampedEntity
{
    [ForeignKey(nameof(TokenComponentId))]
    public Guid TokenComponentId { get; set; }
    public TokenComponent? TokenComponent { get; set; }
    public int Position { get; set; }

    [ForeignKey(nameof(VerseId))]
    public Guid VerseId { get; set; }
    public Verse? Verse { get; set; }

    public DateTimeOffset? Deleted { get; set; }
}