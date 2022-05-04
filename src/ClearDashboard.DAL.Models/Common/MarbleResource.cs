
namespace ClearDashboard.DataAccessLayer.Models
{
    public class MarbleResource
    {
        public int Id { get; set; }
        public int TotalSenses { get; set; }
        public string Word { get; set; } = string.Empty;
        public string WordTransliterated { get; set; }
        public string SenseId { get; set; } = string.Empty;
        public string Domains { get; set; } = string.Empty;
        public string SubDomains { get; set; } = string.Empty;
        public string Glosses { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string Strong { get; set; } = string.Empty;
        public string DefinitionLong { get; set; } = string.Empty;
        public string DefinitionShort { get; set; } = string.Empty;
        public string PoS { get; set; } = string.Empty;
        public string LogosRef { get; set; } = string.Empty;
        public bool IsSense { get; set; } = false;
    }

}
