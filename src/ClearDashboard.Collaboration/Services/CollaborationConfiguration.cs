using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Services
{
    public class CollaborationConfiguration
    {
        public string RemoteUrl { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string RemoteUserName { get; set; } = string.Empty;
        public string RemoteEmail { get; set; } = string.Empty;
        public string RemotePersonalAccessToken { get; set; } = string.Empty;
        public string RemotePersonalPassword { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

    }
}
