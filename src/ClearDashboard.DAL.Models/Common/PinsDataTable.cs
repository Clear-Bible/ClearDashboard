using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class PinsDataTable
    {
        // 0. Source, 1. Lform, 2. Gloss, 3. Lang, 4. Refs, 5. Code, 6. Match, 7. Notes, 8. SimpRefs, 
        // 9. Phrase, 10. Word, 11. Prefix, 12. Stem, 13. Suffix
        public string Source { get; set; }

        public string Lform { get; set; }

        public string Gloss { get; set; }

        public string Lang { get; set; }

        public string Refs { get; set; }

        public string Code { get; set; }

        public string Match { get; set; }

        public string Notes { get; set; }

        public string SimpRefs { get; set; }

        public string Phrase { get; set; }

        public string Word { get; set; }

        public string Prefix { get; set; }

        public string Stem { get; set; }

        public string Suffix { get; set; }
    }
}
