using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.Events
{
    public class CreateProjectEvent
    {
        public DashboardProject DashboardProject;
        public CreateProjectEvent(DashboardProject DashboardProject)
        {
            this.DashboardProject = DashboardProject;
        }
    }
}
