namespace ClearDashboard.DataAccessLayer.Models;

public interface IDataAssociation 
{
    string AssociationId { get; set; }
    string AssociationType { get; set; }
}

public class DataAssociation : IDataAssociation
{
    public string AssociationId { get; set; }
    public string AssociationType { get; set; }
}