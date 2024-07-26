namespace ClearDashboard.DataAccessLayer.Data.Settings;

public class ConnectionStringsOptions
{
    public static readonly string Section = Options.RemoveOptionsSuffix(nameof(ConnectionStringsOptions));

    public string ClearDashboardDatabase { get; set; } = String.Empty;
}