
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Note
    {
        public Note()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            ContentCollection = new HashSet<IContent>();
            Anchors = new HashSet<IAnchor>();
            RecipientNoteUsers = new HashSet<IRecipientNoteUser>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public virtual ICollection<IContent>? ContentCollection { get; set; }
        public virtual ICollection<IRecipientNoteUser>? RecipientNoteUsers { get; set; }
        public virtual User? Author { get; set; }
        public int AuthorId { get; set; }
        public virtual ICollection<IAnchor> Anchors { get; set; }
    

    }
}
