namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMapping : SynchronizableTimestampedEntity
{
    public VerseMapping()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        VerseMappingVerseAssociations = new HashSet<VerseMappingVerseAssociation>();
        VerseMappingTokenizationsAssociations = new HashSet<VerseMappingTokenizationsAssociation>();

        // ReSharper restore VirtualMemberCallInConstructor
    }

    public virtual Guid ParallelCorpusVersionId { get; set; }
    public virtual ParallelCorpusVersion ParallelCorpusVersion { get; set; }

    public virtual ICollection<VerseMappingTokenizationsAssociation> VerseMappingTokenizationsAssociations { get; set; }

    public virtual ICollection<VerseMappingVerseAssociation> VerseMappingVerseAssociations { get; set; }
}