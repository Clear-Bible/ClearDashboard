namespace ClearDashboard.DataAccessLayer.Models;

public partial class RecipientNoteUser : ClearEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }
    public UserType UserType { get; set; }
}