
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Anchor 
    {
        public VerseRefStart? VerseRefStart { get; set; }

        public VerseRefEnd? VerseRefEnd { get; set; }

        public string? SelectedText { get; set; }

        public int? Offset { get; set; } = null;

        public string? BeforeContext { get; set; } = null;

        public string? AfterContext { get; set; } = null;
    }
}