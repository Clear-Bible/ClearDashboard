
namespace ClearDashboard.DataAccessLayer.Models
{
    public enum PluginConnectionChangeType
    {
        None = 0,
        Closing = 1,
        Restart = 2,
    }

    public class PluginClosing
    {
        public PluginConnectionChangeType PluginConnectionChangeType { get; set; }
    }
}
