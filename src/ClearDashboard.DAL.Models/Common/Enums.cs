namespace ClearDashboard.DataAccessLayer.Models
{
    public enum ViewType
    {
        Target,
        Lemma
    }

    public enum PermissionLevel
    {
        None, // none 0
        ReadOnly, // developer 30
        ReadWrite, // maintainer 40
        Owner // owner 50
    }
}
