namespace ClearDashboard.DataAccessLayer.Models;

public class Tokenization : SynchronizableTimestampedEntity
{
    public Tokenization()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Tokens = new HashSet<Token>();
        SourceVerseMappingTokenizationsAssociations = new HashSet<VerseMappingTokenizationsAssociation>();
        TargetVerseMappingTokenizationsAssociations = new HashSet<VerseMappingTokenizationsAssociation>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public virtual ICollection<Token> Tokens { get; set; }

    public virtual ICollection<VerseMappingTokenizationsAssociation> SourceVerseMappingTokenizationsAssociations { get; set; }
    public virtual ICollection<VerseMappingTokenizationsAssociation> TargetVerseMappingTokenizationsAssociations { get; set; }

    public string? TokenizationFunction { get; set; }
}