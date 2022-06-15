
namespace ClearDashboard.DataAccessLayer.Models
{
    public class AlignmentVersion : SynchronizableTimestampedEntity
    {
        public AlignmentVersion()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Alignments = new HashSet<Alignment>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        
        public bool IsDirty { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Alignment> Alignments { get; set; }
    }
}
