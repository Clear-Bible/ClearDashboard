using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LicenseUser : IdentifiableEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LicenseKey { get; set; }
        public string FullName => $"{FirstName}, {LastName}";
        public string? Id { get; set; }
        public string ParatextUserName { get; set; } = null;
    }
}
