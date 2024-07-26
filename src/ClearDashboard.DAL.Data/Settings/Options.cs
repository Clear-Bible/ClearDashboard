namespace ClearDashboard.DataAccessLayer.Data.Settings;

public class Options
{
    internal static string RemoveOptionsSuffix(string className) => className[..^nameof(Options).Length];
}