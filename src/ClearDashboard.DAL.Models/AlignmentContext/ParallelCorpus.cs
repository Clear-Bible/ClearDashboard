using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParallelCorpus
    {
        public ParallelCorpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Versions = new HashSet<ParallelCorpusVersion>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual ICollection<ParallelCorpusVersion> Versions { get; set; }
    }
}
