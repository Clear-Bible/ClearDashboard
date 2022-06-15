using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParallelCorpus : SynchronizableTimestampedEntity
    {
        public ParallelCorpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Versions = new HashSet<ParallelCorpusVersion>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

       
        public virtual ICollection<ParallelCorpusVersion> Versions { get; set; }
    }
}
