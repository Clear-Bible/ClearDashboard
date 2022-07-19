using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LicenseUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LicenseKey { get; set; }
        public string FullName
        {
            get
            {
                return string.Format("{1}, {0}", FirstName, LastName);
            }
        }
        public string ParatextUserName { get; set; } = null;
        public string LastAlignmentLevelId { get; set; } = null;
        public List<object> AlignmentVersions { get; set; }
        public List<object> AlignmentSets { get; set; }
        public string Id { get; set; }
    }
}
