namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMapping : SynchronizableTimestampedEntity
{
    public VerseMapping()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        VerseMappingVerseAssociations = new HashSet<VerseMappingVerseAssociation>();
       

        // ReSharper restore VirtualMemberCallInConstructor
    }

    public virtual Guid? ParallelCorpusVersionId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }

    public virtual ICollection<VerseMappingVerseAssociation>? VerseMappingVerseAssociations { get; set; }
}