using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class UsfmVerse
    {
        public string Chapter { get; set; } = "";
        public string Verse { get; set; } = "";
        public string Text { get; set; } = "";
        public bool isSentenceStart { get; set; } = false;
    }
}
