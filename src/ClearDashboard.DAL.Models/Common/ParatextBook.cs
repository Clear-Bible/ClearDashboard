namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParatextBook
    {
        public string? BookId { get; set; }
        public string? BookNameShort { get; set; }
        public bool Available { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int USFM_Num { get; set; }

    }
}
