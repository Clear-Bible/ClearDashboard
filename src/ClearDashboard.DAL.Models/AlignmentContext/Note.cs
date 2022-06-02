
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Note : ClearEntity
    {
        public Note()
        {
            // ReSharper disable VirtualMemberCallInConstructor

            Associations = new HashSet<DataAssociation>();
            ContentCollection = new HashSet<RawContent>();
            NoteRecipients = new HashSet<NoteRecipient>();

            // ReSharper restore VirtualMemberCallInConstructor
        }

        public virtual User? Author { get; set; }
        public int AuthorId { get; set; }

        public virtual ICollection<DataAssociation> Associations { get; set; }
        public virtual ICollection<RawContent> ContentCollection { get; set; }
        public virtual ICollection<NoteRecipient> NoteRecipients { get; set; }

        [NotMapped]
        public IEnumerable<StringContent> StringContentCollection => ContentCollection.Where(content => content.ContentType == nameof(StringContent)).Cast<StringContent>();

        [NotMapped]
        public IEnumerable<BinaryContent> BinaryContentCollection => ContentCollection.Where(content => content.ContentType == nameof(BinaryContent)).Cast<BinaryContent>();




    }
}
