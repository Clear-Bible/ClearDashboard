using SIL.Scripture;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class VerseRow : IdentifiableEntity
    {
        public VerseRow() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenComponents = new HashSet<TokenComponent>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? BookChapterVerse { get; set; }

        public string? OriginalText { get; set; }
        public bool IsSentenceStart { get; set; }
        public bool IsInRange { get; set; }
        public bool IsRangeStart { get; set; }
        public bool IsEmpty { get; set; }

        // FIXME:  is it possible for a VerseRow/TokensTextRow to have
        // a different Versification than its parent TokenizedCorpus?
        //public int VerseRef_ScrVersType { get; set; }
        //public string? VerseRef_CustomVersData { get; set; }

        [ForeignKey("TokenizationId")]
        public Guid TokenizationId { get; set; }
        public virtual TokenizedCorpus? Tokenization { get; set; }
        public virtual ICollection<TokenComponent> TokenComponents { get; set; }
    }
}
