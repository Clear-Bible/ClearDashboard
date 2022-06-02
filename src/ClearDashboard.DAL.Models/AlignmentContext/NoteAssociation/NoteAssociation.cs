namespace ClearDashboard.DataAccessLayer.Models;

public partial class NoteAssociation : ClearEntity
{
    public string? AssociationId { get; set; }
    public string? AssociationType { get; set; }
}