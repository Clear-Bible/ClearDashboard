namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ParallelCorpus : ClearEntity
    {
        public ParallelCorpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            ParallelVersesLinks = new HashSet<ParallelVersesLink>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public Guid SourceCorpusId { get; set; }
        public Guid TargetCorpusId { get; set; }

        public AlignmentType AlignmentType { get; set; }
  
        public DateTimeOffset LastGenerated { get; set; }


        public virtual Corpus? SourceCorpus { get; set; }
        public virtual Corpus? TargetCorpus { get; set; }
        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks { get; set; }
    }
}
