namespace ClearDashboard.DataAccessLayer.Models;

public abstract class SynchronizableEntity : IdentifiableEntity
{
    public Guid? ParentId { get; set; }
    public Guid  UserId { get; set; }
}