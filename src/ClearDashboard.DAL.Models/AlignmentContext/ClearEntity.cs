namespace ClearDashboard.DataAccessLayer.Models;

public abstract class ClearEntity 
{
    public int Id { get; set; }
    public string? Discriminator { get; set; }
}