namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentSet : IdentifiableEntity
{
    public AlignmentSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        AlignmentTokensPairs = new HashSet<AlignmentTokenPair>();
        var utcNow = DateTimeOffset.UtcNow;
        Created = utcNow;
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    /// <summary>
    /// Gets or sets the time and date that the entity was created (in UTC).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    public virtual ICollection<AlignmentTokenPair> AlignmentTokensPairs { get; set; }
}