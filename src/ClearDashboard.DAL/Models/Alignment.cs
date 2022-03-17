using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Alignment
    {
        public long Id { get; set; }
        public long SourceTokenId { get; set; }
        public long TargetTokenId { get; set; }
        public byte[] Score { get; set; }
        public long? AlignmentVersionId { get; set; }
        public long? AlignmentTypeId { get; set; }

        public virtual AlignmentType AlignmentType { get; set; }
        public virtual AlignmentVersion AlignmentVersion { get; set; }
        public virtual Token TokenToken1 { get; set; }
        public virtual Token TokenTokenNavigation { get; set; }
    }
}
