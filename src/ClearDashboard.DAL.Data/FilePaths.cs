namespace ClearDashboard.DataAccessLayer.Data
{
    public static class FilePathTemplates
    {
        public static string ProjectBaseDirectory = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "ClearDashboard_Projects");

        public static string ProjectDirectoryTemplate = Path.Combine($"{ProjectBaseDirectory}", $"{{0}}");

        public static string CollabBaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
