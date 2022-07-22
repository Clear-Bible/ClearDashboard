using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Models
{
    public class LicenseUser : IdentifiableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LicenseKey { get; set; }
        public string FullName => $"{FirstName}, {LastName}";
        public string ParatextUserName { get; set; } = null;
    }
}
