namespace ClearDashboard.DataAccessLayer.Models;

public class SynchronizableTimestampedEntity : SynchronizableEntity
{
    public SynchronizableTimestampedEntity()
    {
        var utcNow = DateTimeOffset.UtcNow;

        // Remove any fractions of a millisecond 
        utcNow = utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerMillisecond));

        Created = utcNow;
    }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; }

}