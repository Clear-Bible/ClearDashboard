using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public class SemanticDomainLookup
    {
        public string Word { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long LineNum { get; set; }
    }
}
