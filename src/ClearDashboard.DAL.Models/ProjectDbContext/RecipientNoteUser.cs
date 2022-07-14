namespace ClearDashboard.DataAccessLayer.Models;

public class NoteRecipient : IdentifiableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public UserType UserType { get; set; }
}