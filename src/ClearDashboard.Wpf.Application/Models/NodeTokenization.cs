using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class NodeTokenization
    {
        public string FriendlyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CorpusId { get; set; } = string.Empty;
        public string MetaData { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
