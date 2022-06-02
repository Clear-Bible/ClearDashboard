
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
                    _verseBbcccvvv = _verseBbcccvvv.PadLeft(9, '0').PadLeft(9, '0');
                }
            }
        }



        public string BookStr
        {
            get
            {
                var book = VerseBBCCCVVV.PadLeft(9,'0').Substring(0, 3);
                return book;
            }
            //set => BookStr = value;
        }

        public string ChapterStr
        {
            get
            {
                var chap = VerseBBCCCVVV.PadLeft(9, '0').Substring(3, 3);
                return chap;
            }
            //set => ChapterStr = value;
        }

        public string VerseStr
        {
            get
            {
                var verse = VerseBBCCCVVV.PadLeft(9, '0').Substring(6, 3);
                return verse;
            }
            //set => VerseStr = value;
        }

        public string VerseId { get; set; } = string.Empty;

        public bool Found { get; set; }

        public void SetVerseFromBBBCCCVVV(string bbbcccvvv)
        {
            bbbcccvvv = bbbcccvvv.PadLeft(9,'0');
            VerseBBCCCVVV = bbbcccvvv;
            SilBookNumber = bbbcccvvv.Substring(0, 3);
            ChapterNumber = bbbcccvvv.Substring(3, 3);
            VerseNumber = bbbcccvvv.Substring(6, 3);
        }
    }
}
