using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class DalLicenseUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LicenseKey { get; set; }
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
        public string? Id { get; set; }
    }
}
