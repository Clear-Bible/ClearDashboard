namespace ClearDashboard.DataAccessLayer.Models;

public class TimestampedEntity : IdentifiableEntity
{
    public TimestampedEntity()
    {
        var utcNow = DateTimeOffset.UtcNow;

        // Remove any fractions of a millisecond 
        utcNow = utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerMillisecond));

        Created = utcNow;
        Modified = utcNow;
    }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; } 

    /// <summary>
    /// Gets or sets the time and date that the entity was last modified (in UTC).
    /// </summary>
    public DateTimeOffset Modified { get; set; } 
}