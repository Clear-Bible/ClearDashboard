
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Verse : ClearEntity
    {

        public Verse()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            ParallelVersesLinks = new HashSet<ParallelVersesLink>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        

        // Add unique constraint for VerseNumber, SilBookNumber and ChapterNumber
        public int? VerseNumber { get; set; }
        public int? BookNumber { get; set; }
        public int? SilBookNumber { get; set; }
        public int? ChapterNumber { get; set; }

        public string? VerseText { get; set; }
        public int? CorpusId { get; set; }

        public virtual Corpus? Corpus { get; set; }
        public virtual Token? Token { get; set; }
        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }


        private string? _verseBbbcccvvv = string.Empty;
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



        public string BookStr
        {
            get
            {
                var book = VerseBBBCCCVVV.PadLeft(9,'0').Substring(0, 3);
                return book;
            }
            //set => BookStr = value;
        }

        public string ChapterStr
        {
            get
            {
                var chap = VerseBBBCCCVVV.PadLeft(9, '0').Substring(3, 3);
                return chap;
            }
            //set => ChapterStr = value;
        }

        public string VerseStr
        {
            get
            {
                var verse = VerseBBBCCCVVV.PadLeft(9, '0').Substring(6, 3);
                return verse;
            }
            //set => VerseStr = value;
        }

        public string VerseId { get; set; } = string.Empty;

        public bool Found { get; set; }

        public void SetVerseFromBBBCCCVVV(string bbbcccvvv)
        {
            bbbcccvvv = bbbcccvvv.PadLeft(9,'0');
            VerseBBBCCCVVV = bbbcccvvv;
            SilBookNumber = Convert.ToInt32(bbbcccvvv.Substring(0, 3));
            BookNumber = SilBookNumber;
            ChapterNumber = Convert.ToInt32(bbbcccvvv.Substring(3, 3));
            VerseNumber = Convert.ToInt32(bbbcccvvv.Substring(6, 3));
        }
    }
}
