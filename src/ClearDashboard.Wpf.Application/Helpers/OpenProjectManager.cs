using ClearDashboard.Wpf.Application.Properties;
using System.Collections.Generic;
using System.Text.Json;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class OpenProjectManager
    {
        public static void AddProjectToOpenProjectList(DashboardProjectManager projectManager)
        {
            var currentlyOpenProjectsList = DeserializeOpenProjectList();

            if (projectManager.CurrentDashboardProject != null && projectManager.CurrentDashboardProject.ShortFilePath != null 
                                                               && projectManager.CurrentDashboardProject.ShortFilePath != string.Empty)
            {
                currentlyOpenProjectsList.Add(projectManager.CurrentDashboardProject.ShortFilePath
                        .Replace(' ','_').Remove(projectManager.CurrentDashboardProject.ShortFilePath.Length-7));

                SaveOpenProjectList(currentlyOpenProjectsList);
            }
        }

        public static void RemoveProjectToOpenProjectList(DashboardProjectManager projectManager)
        {
            var currentlyOpenProjectsList = DeserializeOpenProjectList();

            if (projectManager.CurrentDashboardProject != null && projectManager.CurrentDashboardProject.ShortFilePath != null
                                                               && projectManager.CurrentDashboardProject.ShortFilePath != string.Empty)
            {
                currentlyOpenProjectsList!.Remove(projectManager.CurrentDashboardProject.ShortFilePath
                    .Replace(' ', '_').Remove(projectManager.CurrentDashboardProject.ShortFilePath.Length-7));

                SaveOpenProjectList(currentlyOpenProjectsList);
            }
        }

        public static void ClearOpenProjectList()
        {
            var currentlyOpenProjectsList = DeserializeOpenProjectList();

            currentlyOpenProjectsList!.Clear();

            SaveOpenProjectList(currentlyOpenProjectsList);
        }

        public static List<string> DeserializeOpenProjectList()
        {
            List<string> currentlyOpenProjectsList = new();
            var currentlyOpenProjectsJson = Settings.Default.CurrentlyOpenProjects;

            if (currentlyOpenProjectsJson != string.Empty)
            {
                currentlyOpenProjectsList = JsonSerializer.Deserialize<List<string>>(currentlyOpenProjectsJson);
            }

            return currentlyOpenProjectsList;
        }

        public static void SaveOpenProjectList(List<string> currentlyOpenProjectsList)
        {
            var currentlyOpenProjectsJson = JsonSerializer.Serialize<List<string>>(currentlyOpenProjectsList);

            Settings.Default.CurrentlyOpenProjects = currentlyOpenProjectsJson;
            Settings.Default.Save();
        }
    }
}
