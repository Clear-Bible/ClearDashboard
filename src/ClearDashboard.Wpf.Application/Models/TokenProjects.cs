using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.Models
{
    public class TokenProject
    {
        public string ParatextProjectId { get; set; }  = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string TokenizationType { get; set; } = string.Empty;
        public Guid CorpusId { get; set; }
        public Guid TokenizedTextCorpusId { get; set; }
        public ParatextProjectMetadata Metadata { get; set; }

        public TokenizedTextCorpus? TokenizedTextCorpus { get; set; }
    }
}
