using ClearDashboard.Wpf.Application.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class OpenProjectManager
    {
        private static string _folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}";
        private static string _fileName = "CurrentlyOpenProjects.txt";
        private static string _filePath = Path.Combine(_folderPath, _fileName);

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

            if (projectManager.CurrentDashboardProject != null && projectManager.CurrentProject.ProjectName != null
                                                               && projectManager.CurrentProject.ProjectName != string.Empty)
            {
                currentlyOpenProjectsList!.Remove(projectManager.CurrentProject.ProjectName
                    .Replace(' ', '_'));

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
            var currentlyOpenProjectsJson = string.Empty;
            List<string> currentlyOpenProjectsList = new();
            if(File.Exists(_filePath))
            {
                currentlyOpenProjectsJson = File.ReadAllText(_filePath);
            }

            if (currentlyOpenProjectsJson != string.Empty)
            {
                currentlyOpenProjectsList = JsonSerializer.Deserialize<List<string>>(currentlyOpenProjectsJson);
            }

            return currentlyOpenProjectsList;
        }

        public static void SaveOpenProjectList(List<string> currentlyOpenProjectsList)
        {
            var currentlyOpenProjectsJson = JsonSerializer.Serialize<List<string>>(currentlyOpenProjectsList);

            File.WriteAllText(_filePath, currentlyOpenProjectsJson);
        }
    }
}
