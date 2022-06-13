using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelVersesLink
    {
        public ParallelVersesLink()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            VerseLinks = new HashSet<VerseLink>();
            // ReSharper restore VirtualMemberCallInConstructor
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
       
        public Guid? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus? ParallelCorpus { get; set; }
        public virtual ICollection<VerseLink> VerseLinks { get; set; }
    }
}
