using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Models
{
    /// <summary>
    /// Follows the Paratext XML for BookNames.xml
    /// </summary>
    public class ParatextBookName
    {
        public string code { get; set; } = "";
        public string abbr { get; set; } = "";
        public string shortname { get; set; } = "";
        public string longname { get; set; } = "";
    }
}
