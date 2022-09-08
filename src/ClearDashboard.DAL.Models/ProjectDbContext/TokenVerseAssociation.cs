namespace ClearDashboard.DataAccessLayer.Models;

public class TokenVerseAssociation : SynchronizableTimestampedEntity
{
    public Guid TokenComponentId { get; set; }
    public TokenComponent? TokenComponent { get; set; }
    public int Position { get; set; }

    public Guid VerseId { get; set; }
    public Verse? Verse { get; set; }
}