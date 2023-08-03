using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateLicenseKeyForDashboard.Models
{
    public class ProjectUserConnection
    {
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        public PermissionLevel AccessLevel { get; set; }

    }
    
}
