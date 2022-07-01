namespace ClearDashboard.DataAccessLayer.Models
{
    public class ScrLanguageWrapper
    {

        public string FontFamily { get; set; } = "Segoe UI";

        public float Size { get; set; } = 13;

        public bool IsRtol { get; set; }

        public ScrLanguage? Language { get; set; }

    }
}
