
using System;
using System.IO;

namespace ClearDashboard.DataAccessLayer.Utility
{
    public class FilePathTemplates
    {

        public static string ProjectBaseDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects";
        public static string ProjectDirectoryTemplate = $"{ProjectBaseDirectory}\\{0}";
    }
}
