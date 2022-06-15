namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMappingVerseAssociation : SynchronizableTimestampedEntity
{
       
    public Guid? VerseMappingId { get; set; }
    public VerseMapping? VerseMapping { get; set; }

    public Guid? VerseId { get; set; }
    public Verse? Verse { get; set; }
}