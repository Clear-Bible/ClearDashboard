namespace ClearDashboard.DataAccessLayer.Data
{
    public static class FilePathTemplates
    {
        public static string ProjectBaseDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects";
        public static string ProjectDirectoryTemplate = $"{ProjectBaseDirectory}\\{{0}}";

        public static string CollabBaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
