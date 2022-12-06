using SIL.Scripture;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class VersificationBookIds
    {
        public ScrVers? Versification { get; set; }
        public IEnumerable<string>? BookAbbreviations { get; set; }
        public List<WorkedOnBook> WorkedOnBooks { get; set; } = new();
    }
}
