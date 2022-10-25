using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class LogReporting
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string ParatextLog { get; set; } = string.Empty;
        public string ClearDashboardLog { get; set; } = string.Empty;
    }
}
