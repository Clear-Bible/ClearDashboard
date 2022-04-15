namespace ClearDashboard.DataAccessLayer.Data
{
    public class FilePathTemplates
    {
        public static string ProjectBaseDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects";
        public static string ProjectDirectoryTemplate = $"{ProjectBaseDirectory}\\{{0}}";
    }
}
