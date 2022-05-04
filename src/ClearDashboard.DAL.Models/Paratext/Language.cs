
using System.ComponentModel;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class SelectedLanguage 
    {
        public string FontFamily { get; set; }

        public double Size { get; set; }

        public string Language { get; set; }

        public string Features { get; set; }

        public string Id { get; set; }

        public bool IsRtoL { get; set; }
    }
}
