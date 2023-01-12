namespace ClearDashboard.DataAccessLayer.Models;

public class SynchronizableTimestampedEntity : SynchronizableEntity
{
    public SynchronizableTimestampedEntity()
    {
        Created = TimestampedEntity.GetUtcNowRoundedToMillisecond();
    }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; }

}