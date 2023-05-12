
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Verse : SynchronizableTimestampedEntity
    {
        public Verse()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenVerseAssociations = new HashSet<TokenVerseAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        // Add unique constraint for VerseNumber, SilBookNumber and ChapterNumber
        public int? VerseNumber { get; set; }

        public int? BookNumber { get; set; }
     
        public int? ChapterNumber { get; set; }

        public string? VerseText { get; set; }

        [ForeignKey(nameof(CorpusId))]
        public Guid? CorpusId { get; set; }
        public virtual Corpus? Corpus { get; set; }

        [ForeignKey(nameof(ParallelCorpusId))]
        public Guid ParallelCorpusId { get; set; }
        public virtual ParallelCorpus? ParallelCorpus { get; set; }

        [ForeignKey(nameof(VerseMappingId))]
        public Guid VerseMappingId { get; set; }
        public VerseMapping? VerseMapping { get; set; }

        public virtual ICollection<TokenVerseAssociation> TokenVerseAssociations { get; set; }

        public string? BBBCCCVVV { get; set; }

    }
}
