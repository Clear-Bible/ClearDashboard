
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Note : TimestampedEntity
    {
        public Note()
        {
            // ReSharper disable VirtualMemberCallInConstructor

            NoteAssociations = new HashSet<NoteAssociation>();
            ContentCollection = new HashSet<RawContent>();
            NoteRecipients = new HashSet<NoteRecipient>();

            // ReSharper restore VirtualMemberCallInConstructor
        }

        public virtual User? Author { get; set; }
        public Guid AuthorId { get; set; }

        public virtual ICollection<NoteAssociation> NoteAssociations { get; set; }
        public virtual ICollection<RawContent> ContentCollection { get; set; }
        public virtual ICollection<NoteRecipient> NoteRecipients { get; set; }

        [NotMapped]
        public IEnumerable<StringContent> StringContentCollection => ContentCollection.Where(content => content.ContentType == nameof(StringContent)).Cast<StringContent>();

        [NotMapped]
        public IEnumerable<BinaryContent> BinaryContentCollection => ContentCollection.Where(content => content.ContentType == nameof(BinaryContent)).Cast<BinaryContent>();

        [NotMapped]
        public IEnumerable<AlignmentAssociation> AlignmentAssociations => NoteAssociations.Where(association => association.AssociationType == nameof(AlignmentAssociation)).Cast<AlignmentAssociation>();

        [NotMapped]
        public IEnumerable<TokenAssociation> TokenAssociations => NoteAssociations.Where(association => association.AssociationType == nameof(TokenAssociation)).Cast<TokenAssociation>();

        [NotMapped]
        public IEnumerable<VerseAssociation> VerseAssociations => NoteAssociations.Where(association => association.AssociationType == nameof(VerseAssociation)).Cast<VerseAssociation>();




    }
}
