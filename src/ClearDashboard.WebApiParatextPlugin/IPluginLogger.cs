using System.Drawing;

namespace ClearDashboard.WebApiParatextPlugin
{
    public interface IPluginLogger
    {
        void AppendText(Color color, string message);
    }
}