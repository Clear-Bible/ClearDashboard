using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class MySqlUser
    {
        public int UserId { get; set; }
        public string RemoteUserName { get; set; }
        public string RemoteEmail { get; set; }
        public string RemotePersonalAccessToken { get; set; }
        public string RemotePersonalPassword { get; set; }
        public string GroupName { get; set; }
        public int NamespaceId { get; set; }
    }
}
