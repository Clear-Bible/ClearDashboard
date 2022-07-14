using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class Token : IdentifiableEntity
    {
        public Token()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            SourceAlignmentTokenPairs = new HashSet<AlignmentTokenPair>();
            TargetAlignmentTokenPairs = new HashSet<AlignmentTokenPair>();
            TokenVerseAssociations = new HashSet<TokenVerseAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }
 
        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
        public int VerseNumber { get; set; }
        public int WordNumber { get; set; }
        public int SubwordNumber { get; set; }

        public Guid TokenizationId { get; set; }
        public virtual TokenizedCorpus? Tokenization { get; set; }

        public string? Text { get; set; }

        public virtual Adornment? Adornment { get; set; }

        public virtual ICollection<AlignmentTokenPair> SourceAlignmentTokenPairs { get; set; }
        public virtual ICollection<AlignmentTokenPair> TargetAlignmentTokenPairs { get; set; }
        public virtual ICollection<TokenVerseAssociation> TokenVerseAssociations { get; set; }

    }
}
