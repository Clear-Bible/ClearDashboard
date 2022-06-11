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
        public int Id { get; set; }
       
        public int? ParallelCorpusId { get; set; }

        public virtual ParallelCorpus? ParallelCorpus { get; set; }
        public virtual ICollection<VerseLink> VerseLinks { get; set; }
    }
}
