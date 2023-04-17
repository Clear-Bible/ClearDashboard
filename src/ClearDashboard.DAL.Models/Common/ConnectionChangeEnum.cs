using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public enum ConnectionChangeType
    {
        None = 0,
        ParatextWindowClosing = 1,
        Restart = 2,
    }

    public class PluginClosing
    {
        public ConnectionChangeType ConnectionChangeType { get; set; } = ConnectionChangeType.None;
    }
}
