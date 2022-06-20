namespace ClearDashboard.DataAccessLayer.Models;

public class SynchronizableTimestampedEntity : SynchronizableEntity
{
    public SynchronizableTimestampedEntity()
    {
        var utcNow = DateTimeOffset.UtcNow;
        Created = utcNow;
        //Modified = utcNow;
    }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Gets or sets the time and date that the entity was last modified (in UTC).
    /// </summary>
    //public DateTimeOffset Modified { get; set; }
}