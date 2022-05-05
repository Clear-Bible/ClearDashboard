
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Verse
    {

        public Verse()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            ParallelVersesLinks = new HashSet<ParallelVersesLink>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public int? Id { get; set; }

        // Add unique constraint for VerseNumber, SilBookNumber and ChapterNumber
        public string? VerseNumber { get; set; }
        public string? SilBookNumber { get; set; }
        public string? ChapterNumber { get; set; }

        public string? VerseText { get; set; }
        public DateTime? LastChanged { get; set; }
        public int? CorpusId { get; set; }

        public virtual Corpus? Corpus { get; set; }
        public virtual Token? Token { get; set; }
        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }


        private string? _verseBbcccvvv = string.Empty;
        public string? VerseBBCCCVVV
        {
            get => _verseBbcccvvv;
            set
            {
                _verseBbcccvvv = value;
                if (_verseBbcccvvv is { Length: < 8 })
                {
                    _verseBbcccvvv = _verseBbcccvvv.PadLeft(8, '0');
                }
            }
        }



        public string BookStr
        {
            get
            {
                var book = VerseBBCCCVVV.Substring(0, 2);
                return book;
            }
        }

        public string ChapterStr
        {
            get
            {
                var chap = VerseBBCCCVVV.Substring(2, 3);
                return chap;
            }
        }

        public string VerseStr
        {
            get
            {
                var verse = VerseBBCCCVVV.Substring(5, 3);
                return verse;
            }
        }

        public string VerseId { get; set; } = string.Empty;

        public bool Found { get; set; }
    }
}
