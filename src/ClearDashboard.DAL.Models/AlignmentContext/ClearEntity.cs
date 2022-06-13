using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public abstract class ClearEntity 
{
    protected ClearEntity()
    {
        var utcNow = DateTimeOffset.UtcNow;
        Created = utcNow;
        Modified = utcNow;
    }
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTimeOffset Created { get; set; } 
    public DateTimeOffset Modified { get; set; }
}