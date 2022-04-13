using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class MARBLEresource
    {
        public int ID { get; set; }
        public int TotalSenses { get; set; }
        public string Word { get; set; } = "";
        public string WordTransliterated { get; set; }
        public string SenseId { get; set; } = "";
        public string Domains { get; set; } = "";
        public string SubDomains { get; set; } = "";
        public string Glosses { get; set; } = "";
        public string Comment { get; set; } = "";
        public string Strong { get; set; } = "";
        public string DefinitionLong { get; set; } = "";
        public string DefinitionShort { get; set; } = "";
        public string PoS { get; set; } = "";
        public string LogosRef { get; set; } = "";
        public bool IsSense { get; set; } = false;
    }

}
