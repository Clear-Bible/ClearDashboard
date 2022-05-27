using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class PinsDataTable
    {
        // 0. Source, 1. Lform, 2. Gloss, 3. Lang, 4. Refs, 5. Code, 6. Match, 7. Notes, 8. SimpRefs, 
        // 9. Phrase, 10. Word, 11. Prefix, 12. Stem, 13. Suffix
        public Guid Id { get; set; }
        public string XmlSource { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Lform { get; set; } = string.Empty;
        public string Gloss { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public string Refs { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Match { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string SimpRefs { get; set; } = string.Empty;
        public string Phrase { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Stem { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public List<string> VerseList { get; set; } = new ();
    }
}
