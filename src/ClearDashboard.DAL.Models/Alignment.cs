
namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Alignment
    {
        public int Id { get; set; }
        public int SourceTokenId { get; set; }
        public int TargetTokenId { get; set; }
        public decimal Score { get; set; }
        public int? AlignmentVersionId { get; set; }
        public virtual AlignmentType AlignmentType { get; set; }
        public virtual AlignmentVersion AlignmentVersion { get; set; }
        public virtual Token SourceToken { get; set; }
        public virtual Token TargetToken { get; set; }
    }
}
