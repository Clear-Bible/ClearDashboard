namespace ClearDashboard.DataAccessLayer.Models;

public partial class NoteRecipient : ClearEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public UserType UserType { get; set; }
}