using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class Verse
    {
        public string VerseID { get; set; } = string.Empty;
        public string VerseText { get; set; } = string.Empty;
        public bool Found { get; set; }
    }
}
