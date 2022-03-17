using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class User
    {
        public User()
        {
            AlignmentVersions = new HashSet<AlignmentVersion>();
        }

        public long Id { get; set; }
        public string ParatextUsername { get; set; }
        public long? LastAlignmentLevelId { get; set; }

        public virtual InterlinearNote UserNavigation { get; set; }
        public virtual ICollection<AlignmentVersion> AlignmentVersions { get; set; }
    }
}
