namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public enum DatabaseType
    {
        SDBH,
        SDBG,
    }

    public class LexicalLink
    {
        public DatabaseType Database { get; set; } = DatabaseType.SDBH;
        public string WordSenseId { get; set; }
        public string Word { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsRtl { get; set; } = false;
    }
}
