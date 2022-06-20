using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class Token : SynchronizableTimestampedEntity
    {
        public Token()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            SourceAlignmentTokenPairs = new HashSet<AlignmentTokenPair>();
            TargetAlignmentTokenPairs = new HashSet<AlignmentTokenPair>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        // add unique constraint for WordNumber and SubwordNumber
        public int WordNumber { get; set; }
        public int SubwordNumber { get; set; }

        public Guid VerseId { get; set; }
        public Verse? Verse { get; set; }

        public Guid TokenizationId { get; set; }
        public virtual Tokenization? Tokenization { get; set; }

        public string? Text { get; set; }

        public string? FirstLetter { get; set; }

        
        public virtual Adornment? Adornment { get; set; }

        public virtual ICollection<AlignmentTokenPair> SourceAlignmentTokenPairs { get; set; }
        public virtual ICollection<AlignmentTokenPair> TargetAlignmentTokenPairs { get; set; }


    }
}
