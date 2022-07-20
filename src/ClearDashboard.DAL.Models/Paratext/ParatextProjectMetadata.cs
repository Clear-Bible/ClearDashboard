using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Paratext
{
    public class ParatextProjectMetadata
    {
        public string Id { get; set; }
        public ProjectType ProjectType { get; set; }
        public CorpusType CorpusType { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string LanguageName { get; set; }
    }
}
