using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using System;

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
