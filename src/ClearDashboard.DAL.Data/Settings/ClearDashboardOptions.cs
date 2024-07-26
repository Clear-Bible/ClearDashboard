namespace ClearDashboard.DataAccessLayer.Data.Settings;

public class ClearDashboardOptions
{
    public static readonly string Section = Options.RemoveOptionsSuffix(nameof(ClearDashboardOptions));

    public string ServerProjectName { get; set; } = String.Empty;
}