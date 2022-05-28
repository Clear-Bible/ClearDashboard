
namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class AlignmentVersion
    {
        public AlignmentVersion()
        {
            Alignments = new HashSet<Alignment>();
        }

        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int? UserId { get; set; }
        public bool IsDirty { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Alignment> Alignments { get; set; }
    }
}
