namespace ClearDashboard.DataAccessLayer.Models;

public abstract class ClearEntity 
{
    protected ClearEntity()
    {
        var utcNow = DateTime.UtcNow;
        Created = utcNow;
        Modified = utcNow;
    }
    public int Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
}