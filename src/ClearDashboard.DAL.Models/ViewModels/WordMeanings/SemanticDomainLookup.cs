using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public class SemanticDomainLookup
    {
        public string Word { get; set; }
        public string FileName { get; set; }
        public long LineNum { get; set; }
    }
}
