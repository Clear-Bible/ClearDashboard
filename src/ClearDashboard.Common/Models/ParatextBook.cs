using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class ParatextBook
    {
        public string BookId { get; set; }
        public string BookNameShort { get; set; }
        public bool Available { get; set; }
        public string FilePath { get; set; }
        public int USFM_Num { get; set; }

    }
}
