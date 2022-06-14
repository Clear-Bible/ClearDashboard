namespace ClearDashboard.DataAccessLayer.Models;

public class Tokenization : IdentifiableEntity
{
    public Tokenization()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Tokens = new HashSet<Token>();
        VerseMappingTokenizationsAssociations = new HashSet<VerseMappingTokenizationsAssociation>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public virtual ICollection<Token> Tokens { get; set; }

    public virtual ICollection<VerseMappingTokenizationsAssociation> VerseMappingTokenizationsAssociations { get; set; }

    public string? TokenizationFunction { get; set; }
}