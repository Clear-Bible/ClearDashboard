
namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParatextProjectMetadata
    {
        public string Id { get; set; }
        public CorpusType CorpusType { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string LanguageName { get; set; }

        public string CorpusTypeDisplay => CorpusType.ToString();
    }
}
