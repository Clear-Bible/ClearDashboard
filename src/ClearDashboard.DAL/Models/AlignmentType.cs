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
}
