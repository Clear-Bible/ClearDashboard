namespace ClearDashboard.DataAccessLayer.Models
{
    public enum ViewType
    {
        Target,
        Lemma
    }

    public enum PermissionLevel
    {
        ReadOnly, // developer 30
        ReadWrite, // maintainer 40
        Owner // owner 50
    }
}
