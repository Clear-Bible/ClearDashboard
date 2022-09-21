namespace ClearDashboard.DataAccessLayer.Models;

public abstract class SynchronizableEntity : IdentifiableEntity
{
    public Guid  UserId { get; set; }
    public virtual User? User { get; set; }
}