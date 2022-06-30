
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Anchor 
    {
        public VerseRefStart? VerseRefStart { get; set; }

        public VerseRefEnd VerseRefEnd { get; set; }

        public string SelectedText { get; set; }

        public int Offset { get; set; }

        public string BeforeContext { get; set; }
        
        public string AfterContext { get; set; }
    }
}