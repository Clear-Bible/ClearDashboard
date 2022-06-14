namespace ClearDashboard.DataAccessLayer.Models;

public partial class NoteAssociation : IdentifiableEntity
{
    public string? AssociationId { get; set; }
    public string? AssociationType { get; set; }
}