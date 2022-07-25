
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

        public Guid? CorpusId { get; set; }
        public virtual Corpus? Corpus { get; set; }
        public Guid? VerseMappingId { get; set; }
        public VerseMapping? VerseMapping { get; set; }

        public virtual ICollection<TokenVerseAssociation> TokenVerseAssociations { get; set; }



        // TODO:  These need to be removed which is going to cause refactoring in many view models.
        private string? _verseBbbcccvvv = string.Empty;
        // ReSharper disable once InconsistentNaming
        public string? VerseBBBCCCVVV
        {
            get => _verseBbbcccvvv;
            set
            {
                _verseBbbcccvvv = value;
                if (_verseBbbcccvvv is { Length: < 8 })
                {
                    _verseBbbcccvvv = _verseBbbcccvvv.PadLeft(9, '0').PadLeft(9, '0');
                }
            }
        }


        [NotMapped]
        public string? SilBookAbbreviation
        {
            get
            {
                var book = VerseBBBCCCVVV?.PadLeft(9, '0').Substring(0, 3);
                return book;
            }
        }

        [NotMapped]
        public string? ChapterStr
        {
            get
            {
                var chap = VerseBBBCCCVVV?.PadLeft(9, '0').Substring(3, 3);
                return chap;
            }
        }

        [NotMapped]
        public string? VerseString
        {
            get
            {
                var verse = VerseBBBCCCVVV?.PadLeft(9, '0').Substring(6, 3);
                return verse;
            }
        }

        //ReSharper disable once InconsistentNaming
        public void SetVerseFromBBBCCCVVV(string bbbcccvvv)
        {
            VerseBBBCCCVVV = bbbcccvvv.PadLeft(9, '0');
            BookNumber = Convert.ToInt32(bbbcccvvv.Substring(0, 3));
            ChapterNumber = Convert.ToInt32(bbbcccvvv.Substring(3, 3));
            VerseNumber = Convert.ToInt32(bbbcccvvv.Substring(6, 3));
        }
    }
}
