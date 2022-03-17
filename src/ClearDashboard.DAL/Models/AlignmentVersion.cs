using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class AlignmentVersion
    {
        public AlignmentVersion()
        {
            Alignments = new HashSet<Alignment>();
        }

        public long Id { get; set; }
        public byte[] CreateDate { get; set; }
        public long? UserId { get; set; }
        public byte[] IsDirty { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Alignment> Alignments { get; set; }
    }
}
