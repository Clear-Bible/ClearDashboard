namespace ClearDashboard.DataAccessLayer.Models
{
    /// <summary>
    /// Follows the Paratext XML for BookNames.xml
    /// </summary>
    public class ParatextBookFileName
    {
        public string code { get; set; } = "";
        public string abbr { get; set; } = "";
        public string shortname { get; set; } = "";
        public string longname { get; set; } = "";

        public string fileID { get; set; } = "";
        public string BB { get; set; } = "";
    }
}
