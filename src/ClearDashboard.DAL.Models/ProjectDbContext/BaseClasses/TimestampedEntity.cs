namespace ClearDashboard.DataAccessLayer.Models;

public class TimestampedEntity : IdentifiableEntity
{
    public TimestampedEntity()
    {
        var utcNow = GetUtcNowRoundedToMillisecond();

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

    public static DateTimeOffset GetUtcNowRoundedToMillisecond()
    {
        var utcNow = DateTimeOffset.UtcNow;
        return utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerMillisecond)); // Remove any fractions of a millisecond
    }
}