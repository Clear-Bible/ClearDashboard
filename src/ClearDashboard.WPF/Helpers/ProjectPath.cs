using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf;

namespace ClearDashboard.Wpf.Helpers
{
    public static class ProjectPath
    {
        public static string GetProjectPath(DashboardProjectManager _projectManager)
        { 
            // check to see if the project directory already exists:
            // get the projects directory
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            appPath = Path.Combine(appPath, "ClearDashboard_Projects", _projectManager.CurrentDashboardProject.ProjectName);

            if (!Directory.Exists(appPath))
            {
                // directory doesn't exist
                Directory.CreateDirectory(appPath);
            }

            return appPath;
        }
    }
}
