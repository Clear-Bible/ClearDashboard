
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Note
    {
        public Note()
        {
            // ReSharper disable VirtualMemberCallInConstructor

            //Associations = new HashSet<DataAssociation>();
            ContentCollection = new HashSet<RawContent>();
            RecipientNoteUsers = new HashSet<RecipientNoteUser>();

            // ReSharper restore VirtualMemberCallInConstructor
        }

        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
      
        public virtual User? Author { get; set; }
        public int AuthorId { get; set; }

        //public virtual ICollection<DataAssociation>? Associations { get; set; }
        public virtual ICollection<RawContent>? ContentCollection { get; set; }
        public virtual ICollection<RecipientNoteUser>? RecipientNoteUsers { get; set; }



    }
}
