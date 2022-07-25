
using System.Diagnostics;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class UserPreferences
    {
        public int ValidationLevel = 0;

        public ViewType ViewType { get; set; } = ViewType.Target;
        public bool IsOt { get; set; } = true;

        public string AlignmentLastVerse { get; set; } = "";

        public string AlignmentSuggestionLastVerse { get; set; } = "";

        public string SelectedGlossShortName { get; set; } = "";

        public List<SelectedBook> SelectedBooks { get; set; } = new List<SelectedBook>();

        public List<string> GlossLicenses { get; set; } = new List<string>();

        private int _lastContentWordLevel;

        public int LastContentWordLevel
        {
            get => _lastContentWordLevel;
            set
            {
                if (value < 0)
                {
                    _lastContentWordLevel = 0;
                }
                else if (value > 5)
                {
                    _lastContentWordLevel = 5;
                }
                else
                {
                    _lastContentWordLevel = value;
                }
            }
        }


        public static UserPreferences LoadUserPreferencesFile(DashboardProject dashboardProject)
        {
            var fileInfo = new FileInfo(dashboardProject.FullFilePath);
            var path = Path.Combine(fileInfo.DirectoryName, "prefs.jsn");

            // make a default version
            var userPreferences = new UserPreferences
            {
                ValidationLevel = 0
            };

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                userPreferences = JsonSerializer.Deserialize<UserPreferences>(json);

                //check for null meaning that there was a problem with the file
                if (userPreferences is null)
                {
                    var upNew = new UserPreferences();
                    SaveUserPreferencesFile(upNew, new DashboardProject
                    {
                        FullFilePath = path,
                    });
                    userPreferences = upNew;
                }

                if (userPreferences.SelectedBooks is null)
                {
                    userPreferences.SelectedBooks = new List<SelectedBook>();
                }

                return userPreferences;
            }

            return userPreferences;
        }

        public static bool SaveUserPreferencesFile(UserPreferences userPreferences, DashboardProject dashboardProject)
        {
            var newUserPreferences = new UserPreferences
            {
                LastContentWordLevel = userPreferences.LastContentWordLevel,
                IsOt = userPreferences.IsOt,
                ValidationLevel = userPreferences.ValidationLevel,
                ViewType = userPreferences.ViewType,
                AlignmentLastVerse = userPreferences.AlignmentLastVerse,
                GlossLicenses = userPreferences.GlossLicenses,
                SelectedGlossShortName = userPreferences.SelectedGlossShortName
            };

            if (dashboardProject.FullFilePath != null)
            {
                var fileInfo = new FileInfo(dashboardProject.FullFilePath);
                if (fileInfo.DirectoryName != null)
                {
                    var path = Path.Combine(fileInfo.DirectoryName, "prefs.jsn");

                    try
                    {
                        using var createStream = File.Create(path);
                        JsonSerializer.Serialize(createStream, newUserPreferences);
                        createStream.Dispose();
                        //// serialize JSON directly to a file
                        //using (StreamWriter file = File.CreateText(path))
                        //{
                        //    var serializer = new JsonSerializerOptions
                        //    {
                        
                        //        Formatting = Newtonsoft.Json.Formatting.Indented
                        //    };
                        //    serializer.Serialize(file, upNew);
                        //}
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
