namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMapping : IdentifiableEntity
{
    public VerseMapping()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        VerseMappingVerseAssociations = new HashSet<VerseMappingVerseAssociation>();
        VerseMappingTokenizationsAssociations = new HashSet<VerseMappingTokenizationsAssociation>();

        // ReSharper restore VirtualMemberCallInConstructor
    }

    public virtual ICollection<VerseMappingTokenizationsAssociation> VerseMappingTokenizationsAssociations { get; set; }

    public virtual ICollection<VerseMappingVerseAssociation> VerseMappingVerseAssociations { get; set; }
}