namespace ClearDashboard.DataAccessLayer.Models;

public interface IRecipientNoteUser 
{
    int? UserId { get; set; }
    User? User { get; set; }
    UserType UserType { get; set; }
}

public class RecipientNoteUser : ClearEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }
    public UserType UserType { get; set; }
}