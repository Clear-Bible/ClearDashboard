
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Alignment : SynchronizableTimestampedEntity
    {
        public Guid SourceTokenId { get; set; }
        public Guid TargetTokenId { get; set; }
        public decimal Score { get; set; }
        public Guid? AlignmentVersionId { get; set; }
        //public virtual AlignmentType AlignmentType { get; set; }
        public virtual AlignmentVersion? AlignmentVersion { get; set; }
        public virtual Token? SourceToken { get; set; }
        public virtual Token? TargetToken { get; set; }

    }
}
