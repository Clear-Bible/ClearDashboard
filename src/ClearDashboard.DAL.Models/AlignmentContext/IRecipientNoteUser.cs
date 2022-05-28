namespace ClearDashboard.DataAccessLayer.Models;

public interface IRecipientNoteUser 
{
    int? UserId { get; set; }
    User? User { get; set; }
    UserType UserType { get; set; }
}