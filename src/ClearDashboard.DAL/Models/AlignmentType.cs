using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class AlignmentType
    {
        public AlignmentType()
        {
            Alignments = new HashSet<Alignment>();
        }

        public int Id { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Alignment> Alignments { get; set; }
    }

    public enum EAlignType
    {
        Auto = 1,
        Manual = 2,
    }
    public enum EAlignState
    {
        NotSet = -1,
        NotChecked = 0,
        Verified = 1,
        Bad = 2,
        Question = 3,
    }


}
