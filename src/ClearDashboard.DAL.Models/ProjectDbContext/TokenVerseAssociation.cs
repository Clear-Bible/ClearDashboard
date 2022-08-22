namespace ClearDashboard.DataAccessLayer.Models;

public class TokenVerseAssociation : SynchronizableTimestampedEntity
{
    public Guid TokenId { get; set; }
    public Token? Token { get; set; }
    public int Position { get; set; }

    public Guid VerseId { get; set; }
    public Verse? Verse { get; set; }
}