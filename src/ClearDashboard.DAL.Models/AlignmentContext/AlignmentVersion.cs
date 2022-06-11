
namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class AlignmentVersion : ClearEntity
    {
        public AlignmentVersion()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Alignments = new HashSet<Alignment>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        
        public int? UserId { get; set; }
        public bool IsDirty { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Alignment> Alignments { get; set; }
    }
}
